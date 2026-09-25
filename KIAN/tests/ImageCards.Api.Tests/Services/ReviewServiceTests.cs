using ImageCards.Api.DTOs;
using ImageCards.Api.Exceptions;
using ImageCards.Api.Models;
using ImageCards.Api.Services.SpacedRepetition;
using ImageCards.Api.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using static ImageCards.Api.Tests.Services.ServiceTestContext;

namespace ImageCards.Api.Tests.Services;

public sealed class ReviewServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    private Task<ReviewStateDto> Review(Guid cardId, ReviewRating rating) =>
        _ctx.ReviewService().SubmitReviewAsync(new SubmitReviewRequestDto { CardId = cardId, Rating = rating }, default);

    private Task<DueCardsResponseDto> Due(string language = "en", int limit = 50) =>
        _ctx.ReviewService().GetDueCardsAsync(language, limit, default);

    [Fact]
    public async Task GetDueCards_ReturnsNewCardsAsFlashcardsInRequestedLanguage()
    {
        var card = await _ctx.CreateCardAsync(Translation("en", "Apple", "I eat an apple."), Translation("es", "Manzana"));

        var due = await Due("es");

        Assert.Equal(1, due.TotalDue);
        var flashcard = Assert.Single(due.Cards);
        Assert.Equal(card.Id, flashcard.Id);
        Assert.Equal("Manzana", flashcard.Translation);
        Assert.Equal("es", flashcard.Language);
        Assert.Null(flashcard.ExampleSentence);
        Assert.Equal(card.ImageUrl, flashcard.ImageUrl);
    }

    [Fact]
    public async Task GetDueCards_ExcludesCardsWithoutTranslationInLanguage()
    {
        await _ctx.CreateCardAsync(Translation("en", "Apple"));

        var due = await Due("fr");

        Assert.Equal(0, due.TotalDue);
        Assert.Empty(due.Cards);
    }

    [Fact]
    public async Task GetDueCards_ExcludesCardsScheduledInTheFuture_UntilTheyAreDue()
    {
        var card = await _ctx.CreateCardAsync();
        await Review(card.Id, ReviewRating.Good); // next review in 1 day

        Assert.Empty((await Due()).Cards);

        _ctx.Time.Advance(TimeSpan.FromDays(1));
        Assert.Equal(card.Id, Assert.Single((await Due()).Cards).Id);
    }

    [Fact]
    public async Task GetDueCards_OrdersByNextReviewAndAppliesLimit()
    {
        var older = await _ctx.CreateCardAsync(Translation("en", "Apple"));
        _ctx.Time.Advance(TimeSpan.FromMinutes(1));
        var newer = await _ctx.CreateCardAsync(Translation("en", "Pear"));
        _ctx.Time.Advance(TimeSpan.FromMinutes(1));
        await _ctx.CreateCardAsync(Translation("en", "Plum"));

        var due = await Due(limit: 2);

        Assert.Equal(3, due.TotalDue);
        Assert.Equal([older.Id, newer.Id], due.Cards.Select(c => c.Id));
    }

    [Fact]
    public async Task GetDueCards_ExcludesOtherUsersCards()
    {
        await _ctx.CreateCardAsync();
        _ctx.CurrentUser.UserId = TestUsers.Bob;

        Assert.Empty((await Due()).Cards);
    }

    [Fact]
    public async Task SubmitReview_Good_PersistsScheduledState()
    {
        var card = await _ctx.CreateCardAsync();

        var result = await Review(card.Id, ReviewRating.Good);

        Assert.Equal(card.Id, result.CardId);
        Assert.Equal(1, result.IntervalDays);
        Assert.Equal(1, result.Repetitions);
        Assert.Equal(TestClock.Start.UtcDateTime.AddDays(1), result.NextReviewAt);
        Assert.Equal(TestClock.Start.UtcDateTime, result.LastReviewedAt);

        var stored = await _ctx.NewDbContext().CardReviews.SingleAsync();
        Assert.Equal(result.NextReviewAt, stored.NextReviewAt);
        Assert.Equal(DateTimeKind.Utc, stored.NextReviewAt.Kind);
    }

    [Fact]
    public async Task SubmitReview_Again_MakesCardDueAgainAfterRelearnDelay()
    {
        var card = await _ctx.CreateCardAsync();
        await Review(card.Id, ReviewRating.Good);
        _ctx.Time.Advance(TimeSpan.FromDays(1));

        var result = await Review(card.Id, ReviewRating.Again);

        Assert.Equal(1, result.Lapses);
        Assert.Empty((await Due()).Cards);
        _ctx.Time.Advance(Sm2SpacedRepetitionScheduler.RelearnDelay);
        Assert.Single((await Due()).Cards);
    }

    [Fact]
    public async Task SubmitReview_Sequence_GrowsInterval()
    {
        var card = await _ctx.CreateCardAsync();

        var first = await Review(card.Id, ReviewRating.Good);
        _ctx.Time.SetUtcNow(first.NextReviewAt);
        var second = await Review(card.Id, ReviewRating.Good);
        _ctx.Time.SetUtcNow(second.NextReviewAt);
        var third = await Review(card.Id, ReviewRating.Easy);

        Assert.Equal([1, 6, 21],new[] { first.IntervalDays, second.IntervalDays, third.IntervalDays });
        Assert.Equal(2.65, third.EaseFactor, precision: 10);
    }

    [Fact]
    public async Task SubmitReview_ForUnknownCard_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Review(Guid.NewGuid(), ReviewRating.Good));
    }

    [Fact]
    public async Task SubmitReview_ForAnotherUsersCard_ThrowsNotFoundAndLeavesStateUntouched()
    {
        var card = await _ctx.CreateCardAsync();
        _ctx.CurrentUser.UserId = TestUsers.Bob;

        await Assert.ThrowsAsync<NotFoundException>(() => Review(card.Id, ReviewRating.Easy));

        var db = _ctx.NewDbContext();
        var review = await db.CardReviews.SingleAsync();
        Assert.Equal(TestUsers.Alice, review.UserId);
        Assert.Equal(0, review.Repetitions);
    }

    [Fact]
    public async Task SubmitReview_ForCardWithoutReviewState_CreatesIt()
    {
        var card = await _ctx.CreateCardAsync();
        var db = _ctx.NewDbContext();
        await db.CardReviews.ExecuteDeleteAsync();

        var result = await Review(card.Id, ReviewRating.Good);

        Assert.Equal(1, result.Repetitions);
        Assert.Single(await _ctx.NewDbContext().CardReviews.ToListAsync());
    }

    [Fact]
    public async Task GetReviewState_ReturnsCurrentState()
    {
        var card = await _ctx.CreateCardAsync();
        await Review(card.Id, ReviewRating.Hard);

        var state = await _ctx.ReviewService().GetReviewStateAsync(card.Id, default);

        Assert.Equal(1, state.IntervalDays);
        Assert.Equal(2.35, state.EaseFactor, precision: 10);
    }

    [Fact]
    public async Task GetReviewState_ForUnknownCard_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _ctx.ReviewService().GetReviewStateAsync(Guid.NewGuid(), default));
    }
}
