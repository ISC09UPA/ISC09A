using ImageCards.Api.Data;
using ImageCards.Api.DTOs;
using ImageCards.Api.Exceptions;
using ImageCards.Api.Models;
using ImageCards.Api.Services.CurrentUser;
using ImageCards.Api.Services.Images;
using ImageCards.Api.Services.SpacedRepetition;
using ImageCards.Api.Services.Storage;
using Microsoft.EntityFrameworkCore;

namespace ImageCards.Api.Services.Cards;

public sealed class CardService(
    ImageCardsDbContext db,
    ICurrentUserService currentUser,
    IBlobStorageService blobStorage,
    IImageValidator imageValidator,
    ISpacedRepetitionScheduler scheduler,
    TimeProvider timeProvider,
    ILogger<CardService> logger) : ICardService
{
    private const string CardResource = "Card";
    private const string ImageField = "image";

    public async Task<PagedResponse<CardResponseDto>> GetCardsAsync(
        CardQueryParameters query, CancellationToken cancellationToken)
    {
        var language = NormalizeOptional(query.Language);
        var cards = OwnedCards();
        if (language is not null)
        {
            cards = cards.Where(c => c.Translations.Any(t => t.LanguageCode == language));
        }

        var total = await cards.CountAsync(cancellationToken);
        var page = await cards
            .Include(c => c.Translations)
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var items = page.Select(c => ToDto(c, language)).ToList();
        return new PagedResponse<CardResponseDto>(items, query.Page, query.PageSize, total);
    }

    public async Task<CardResponseDto> GetCardAsync(Guid id, string? language, CancellationToken cancellationToken)
    {
        var card = await FindOwnedCardAsync(id, tracking: false, cancellationToken);
        var normalized = NormalizeOptional(language);
        if (normalized is not null && card.Translations.All(t => t.LanguageCode != normalized))
        {
            throw new NotFoundException("Translation", $"{id}/{normalized}");
        }

        return ToDto(card, normalized);
    }

    public async Task<TranslationDto> GetTranslationAsync(Guid id, string language, CancellationToken cancellationToken)
    {
        var card = await GetCardAsync(id, language, cancellationToken);
        return card.Translations.Single();
    }

    public async Task<CardResponseDto> CreateCardAsync(CreateCardRequestDto request, CancellationToken cancellationToken)
    {
        var image = request.Image ?? throw new RequestValidationException(ImageField, "An image is required.");
        var blobName = await UploadImageAsync(image, cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var card = new Card
        {
            UserId = currentUser.UserId,
            ImageBlobName = blobName,
            CreatedAt = now,
            Translations = ToEntities(request.Translations),
        };
        card.Reviews.Add(NewReview(card, now));
        db.Cards.Add(card);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Do not leave an orphan blob if the card could not be stored.
            await TryDeleteBlobAsync(blobName);
            throw;
        }

        logger.LogInformation("Created card {CardId} for user {UserId}", card.Id, card.UserId);
        return ToDto(card, language: null);
    }

    public async Task<CardResponseDto> UpdateCardAsync(
        Guid id, UpdateCardRequestDto request, CancellationToken cancellationToken)
    {
        var card = await FindOwnedCardAsync(id, tracking: true, cancellationToken);

        string? previousBlobName = null;
        if (request.Image is not null)
        {
            previousBlobName = card.ImageBlobName;
            card.ImageBlobName = await UploadImageAsync(request.Image, cancellationToken);
        }

        ReplaceTranslations(card, request.Translations);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (previousBlobName is not null)
            {
                await TryDeleteBlobAsync(card.ImageBlobName);
            }

            throw;
        }

        if (previousBlobName is not null)
        {
            await TryDeleteBlobAsync(previousBlobName);
        }

        logger.LogInformation("Updated card {CardId} (image replaced: {ImageReplaced})", card.Id, previousBlobName is not null);
        return ToDto(card, language: null);
    }

    public async Task DeleteCardAsync(Guid id, CancellationToken cancellationToken)
    {
        var card = await FindOwnedCardAsync(id, tracking: true, cancellationToken);
        db.Cards.Remove(card);
        await db.SaveChangesAsync(cancellationToken);

        // The database is the source of truth; a leftover blob is harmless and only logged.
        await TryDeleteBlobAsync(card.ImageBlobName);
        logger.LogInformation("Deleted card {CardId}", id);
    }

    private IQueryable<Card> OwnedCards()
    {
        var userId = currentUser.UserId;
        return db.Cards.Where(c => c.UserId == userId);
    }

    private async Task<Card> FindOwnedCardAsync(Guid id, bool tracking, CancellationToken cancellationToken)
    {
        var query = OwnedCards().Include(c => c.Translations).Where(c => c.Id == id);
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        // Cards of other users are reported as not found so their existence is not leaked.
        return await query.SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(CardResource, id);
    }

    private async Task<string> UploadImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        var format = await imageValidator.ValidateAsync(image, ImageField, cancellationToken);
        await using var stream = image.OpenReadStream();
        return await blobStorage.UploadImageAsync(stream, format, cancellationToken);
    }

    private async Task TryDeleteBlobAsync(string blobName)
    {
        try
        {
            // Not tied to the request token: cleanup should finish even if the client disconnected.
            await blobStorage.DeleteImageAsync(blobName, CancellationToken.None);
        }
        catch (Exception ex) when (ex is StorageUnavailableException or ArgumentException)
        {
            logger.LogWarning("Could not delete image blob {BlobName}; it is now orphaned", blobName);
        }
    }

    private CardReview NewReview(Card card, DateTime now)
    {
        var state = scheduler.CreateInitialState(now);
        return new CardReview
        {
            Card = card,
            UserId = card.UserId,
            NextReviewAt = state.NextReviewAt,
            LastReviewedAt = state.LastReviewedAt,
            IntervalDays = state.IntervalDays,
            EaseFactor = state.EaseFactor,
            Repetitions = state.Repetitions,
            Lapses = state.Lapses,
        };
    }

    private void ReplaceTranslations(Card card, IEnumerable<TranslationRequestDto> requested)
    {
        var incoming = ToEntities(requested).ToDictionary(t => t.LanguageCode);

        foreach (var existing in card.Translations.ToList())
        {
            if (incoming.Remove(existing.LanguageCode, out var update))
            {
                existing.TranslatedText = update.TranslatedText;
                existing.ExampleSentence = update.ExampleSentence;
            }
            else
            {
                card.Translations.Remove(existing);
                db.CardTranslations.Remove(existing);
            }
        }

        foreach (var added in incoming.Values)
        {
            card.Translations.Add(added);
        }
    }

    private static List<CardTranslation> ToEntities(IEnumerable<TranslationRequestDto> translations) =>
        translations
            .Select(t => new CardTranslation
            {
                LanguageCode = SupportedLanguages.Normalize(t.Language!),
                TranslatedText = t.TranslatedText!.Trim(),
                ExampleSentence = string.IsNullOrWhiteSpace(t.ExampleSentence) ? null : t.ExampleSentence.Trim(),
            })
            .ToList();

    private CardResponseDto ToDto(Card card, string? language)
    {
        var translations = card.Translations
            .Where(t => language is null || t.LanguageCode == language)
            .OrderBy(t => t.LanguageCode, StringComparer.Ordinal)
            .Select(t => new TranslationDto(t.LanguageCode, t.TranslatedText, t.ExampleSentence))
            .ToList();

        return new CardResponseDto(
            card.Id,
            blobStorage.GetReadUrl(card.ImageBlobName).ToString(),
            card.CreatedAt,
            translations);
    }

    private static string? NormalizeOptional(string? language) =>
        string.IsNullOrWhiteSpace(language) ? null : SupportedLanguages.Normalize(language);
}
