using KIAN.Data;
using KIAN.DTOs;
using KIAN.Models;
using Microsoft.EntityFrameworkCore;

namespace KIAN.Services;

public class CardService
{
    private readonly AppDbContext _db;
    private readonly BlobStorageService _blob;
    private readonly SpacedRepetitionService _sr;
    private readonly CurrentUserService _currentUser;

    public CardService(
        AppDbContext db,
        BlobStorageService blob,
        SpacedRepetitionService sr,
        CurrentUserService currentUser)
    {
        _db = db;
        _blob = blob;
        _sr = sr;
        _currentUser = currentUser;
    }

    private Guid UserId => _currentUser.UserId;

    public async Task<CardResponseDto> CreateAsync(CreateCardDto dto)
    {
        var blobName = await _blob.UploadAsync(dto.Image);

        var card = new Card
        {
            UserId = UserId,
            ImageBlobName = blobName,
            Translations =
            {
                new CardTranslation
                {
                    LanguageCode = Normalize(dto.LanguageCode),
                    TranslatedText = dto.TranslatedText.Trim(),
                    ExampleSentence = dto.ExampleSentence?.Trim()
                }
            },
            Reviews =
            {
                // Disponible para estudiar de inmediato
                new CardReview { UserId = UserId, NextReviewAt = DateTime.UtcNow }
            }
        };

        _db.Cards.Add(card);
        await _db.SaveChangesAsync();

        return Map(card, null);
    }

    public async Task<CardResponseDto> AddTranslationAsync(Guid cardId, AddTranslationDto dto)
    {
        var card = await _db.Cards
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == UserId)
            ?? throw new KeyNotFoundException("Tarjeta no encontrada.");

        var lang = Normalize(dto.LanguageCode);

        if (card.Translations.Any(t => t.LanguageCode == lang))
            throw new InvalidOperationException($"La tarjeta ya tiene una traducción en '{lang}'.");

        card.Translations.Add(new CardTranslation
        {
            LanguageCode = lang,
            TranslatedText = dto.TranslatedText.Trim(),
            ExampleSentence = dto.ExampleSentence?.Trim()
        });

        await _db.SaveChangesAsync();
        return Map(card, null);
    }

    public async Task<List<CardResponseDto>> GetAllAsync(string? language)
    {
        var query = _db.Cards
            .AsNoTracking()
            .Include(c => c.Translations)
            .Where(c => c.UserId == UserId);

        if (!string.IsNullOrWhiteSpace(language))
        {
            var lang = Normalize(language);
            query = query.Where(c => c.Translations.Any(t => t.LanguageCode == lang));
        }

        var cards = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return cards.Select(c => Map(c, language)).ToList();
    }

    public async Task<CardResponseDto?> GetByIdAsync(Guid id)
    {
        var card = await _db.Cards
            .AsNoTracking()
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);

        return card is null ? null : Map(card, null);
    }

    public async Task<List<CardResponseDto>> GetDueAsync(string? language, int limit)
    {
        var now = DateTime.UtcNow;

        var query = _db.CardReviews
            .AsNoTracking()
            .Include(r => r.Card!).ThenInclude(c => c.Translations)
            .Where(r => r.UserId == UserId && r.NextReviewAt <= now);

        if (!string.IsNullOrWhiteSpace(language))
        {
            var lang = Normalize(language);
            query = query.Where(r => r.Card!.Translations.Any(t => t.LanguageCode == lang));
        }

        var reviews = await query
            .OrderBy(r => r.NextReviewAt)
            .Take(limit)
            .ToListAsync();

        return reviews.Select(r => Map(r.Card!, language)).ToList();
    }

    public async Task RegisterReviewAsync(ReviewRequestDto dto)
    {
        var review = await _db.CardReviews
            .FirstOrDefaultAsync(r => r.CardId == dto.CardId && r.UserId == UserId)
            ?? throw new KeyNotFoundException("No existe progreso para esa tarjeta.");

        _sr.Apply(review, dto.Quality);
        await _db.SaveChangesAsync();
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private CardResponseDto Map(Card card, string? language)
    {
        var translations = card.Translations.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(language))
        {
            var lang = Normalize(language);
            translations = translations.Where(t => t.LanguageCode == lang);
        }

        return new CardResponseDto
        {
            Id = card.Id,
            CreatedAt = card.CreatedAt,
            ImageUrl = _blob.GetTemporaryUrl(card.ImageBlobName),
            Translations = translations.Select(t => new TranslationDto
            {
                LanguageCode = t.LanguageCode,
                TranslatedText = t.TranslatedText,
                ExampleSentence = t.ExampleSentence
            }).ToList()
        };
    }
}