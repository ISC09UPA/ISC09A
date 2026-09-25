using ImageCards.Api.Data;
using ImageCards.Api.DTOs;
using ImageCards.Api.Exceptions;
using ImageCards.Api.Models;
using ImageCards.Api.Services.CurrentUser;
using ImageCards.Api.Services.SpacedRepetition;
using ImageCards.Api.Services.Storage;
using Microsoft.EntityFrameworkCore;

namespace ImageCards.Api.Services.Reviews;

public sealed class ReviewService(
    ImageCardsDbContext db,
    ICurrentUserService currentUser,
    IBlobStorageService blobStorage,
    ISpacedRepetitionScheduler scheduler,
    TimeProvider timeProvider,
    ILogger<ReviewService> logger) : IReviewService
{
    private const string CardResource = "Card";

    public async Task<DueCardsResponseDto> GetDueCardsAsync(
        string language, int limit, CancellationToken cancellationToken)
    {
        var languageCode = SupportedLanguages.Normalize(language);
        var userId = currentUser.UserId;
        var now = UtcNow();

        var due = db.CardReviews
            .Where(r => r.UserId == userId
                && r.NextReviewAt <= now
                && r.Card!.Translations.Any(t => t.LanguageCode == languageCode));

        var total = await due.CountAsync(cancellationToken);
        var cards = await due
            .OrderBy(r => r.NextReviewAt)
            .ThenBy(r => r.CardId)
            .Take(limit)
            .Select(r => new
            {
                r.CardId,
                r.NextReviewAt,
                r.Card!.ImageBlobName,
                Translation = r.Card.Translations.First(t => t.LanguageCode == languageCode),
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var flashcards = cards
            .Select(c => new FlashcardDto(
                c.CardId,
                blobStorage.GetReadUrl(c.ImageBlobName).ToString(),
                c.Translation.TranslatedText,
                c.Translation.LanguageCode,
                c.Translation.ExampleSentence,
                c.NextReviewAt))
            .ToList();

        return new DueCardsResponseDto(total, flashcards);
    }

    public async Task<ReviewStateDto> SubmitReviewAsync(
        SubmitReviewRequestDto request, CancellationToken cancellationToken)
    {
        var cardId = request.CardId ?? throw new RequestValidationException(nameof(request.CardId), "The card id is required.");
        var rating = request.Rating ?? throw new RequestValidationException(nameof(request.Rating), "The rating is required.");
        var userId = currentUser.UserId;
        var now = UtcNow();

        var review = await db.CardReviews.SingleOrDefaultAsync(r => r.UserId == userId && r.CardId == cardId, cancellationToken)
            ?? await CreateMissingReviewAsync(userId, cardId, now, cancellationToken);

        var next = scheduler.Schedule(ToState(review), rating, now);
        Apply(review, next);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Card {CardId} reviewed as {Rating}; next review in {IntervalDays} days at {NextReviewAt:O}",
            cardId, rating, next.IntervalDays, next.NextReviewAt);
        return ToDto(review);
    }

    public async Task<ReviewStateDto> GetReviewStateAsync(Guid cardId, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        var review = await db.CardReviews
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.UserId == userId && r.CardId == cardId, cancellationToken)
            ?? throw new NotFoundException(CardResource, cardId);
        return ToDto(review);
    }

    /// <summary>Cards always get a review state on creation; this covers cards created by other means.</summary>
    private async Task<CardReview> CreateMissingReviewAsync(
        Guid userId, Guid cardId, DateTime now, CancellationToken cancellationToken)
    {
        var ownsCard = await db.Cards.AnyAsync(c => c.Id == cardId && c.UserId == userId, cancellationToken);
        if (!ownsCard)
        {
            throw new NotFoundException(CardResource, cardId);
        }

        var review = new CardReview { CardId = cardId, UserId = userId };
        Apply(review, scheduler.CreateInitialState(now));
        db.CardReviews.Add(review);
        return review;
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;

    private static SchedulingState ToState(CardReview review) =>
        new(review.IntervalDays, review.EaseFactor, review.Repetitions, review.Lapses, review.NextReviewAt, review.LastReviewedAt);

    private static void Apply(CardReview review, SchedulingState state)
    {
        review.IntervalDays = state.IntervalDays;
        review.EaseFactor = state.EaseFactor;
        review.Repetitions = state.Repetitions;
        review.Lapses = state.Lapses;
        review.NextReviewAt = state.NextReviewAt;
        review.LastReviewedAt = state.LastReviewedAt;
    }

    private static ReviewStateDto ToDto(CardReview review) =>
        new(review.CardId, review.NextReviewAt, review.LastReviewedAt, review.IntervalDays,
            review.EaseFactor, review.Repetitions, review.Lapses);
}
