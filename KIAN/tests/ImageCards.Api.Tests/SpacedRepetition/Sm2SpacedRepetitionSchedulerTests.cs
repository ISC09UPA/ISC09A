using ImageCards.Api.Models;
using ImageCards.Api.Services.SpacedRepetition;

namespace ImageCards.Api.Tests.SpacedRepetition;

public class Sm2SpacedRepetitionSchedulerTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    private readonly Sm2SpacedRepetitionScheduler _scheduler = new();

    private SchedulingState NewCard => _scheduler.CreateInitialState(Now);

    private static SchedulingState Learned(int interval, int repetitions, double ease = 2.5, int lapses = 0) =>
        new(interval, ease, repetitions, lapses, Now, Now.AddDays(-interval));

    [Fact]
    public void CreateInitialState_ReturnsNewCardDueImmediately()
    {
        var state = _scheduler.CreateInitialState(Now);

        Assert.Equal(0, state.IntervalDays);
        Assert.Equal(Sm2SpacedRepetitionScheduler.DefaultEaseFactor, state.EaseFactor);
        Assert.Equal(0, state.Repetitions);
        Assert.Equal(0, state.Lapses);
        Assert.Equal(Now, state.NextReviewAt);
        Assert.Null(state.LastReviewedAt);
    }

    [Theory]
    [InlineData(ReviewRating.Hard, 1, 2.35)]
    [InlineData(ReviewRating.Good, 1, 2.5)]
    [InlineData(ReviewRating.Easy, 4, 2.65)]
    public void NewCard_SuccessfulRating_SetsFirstIntervalAndEase(ReviewRating rating, int expectedInterval, double expectedEase)
    {
        var state = _scheduler.Schedule(NewCard, rating, Now);

        Assert.Equal(expectedInterval, state.IntervalDays);
        Assert.Equal(expectedEase, state.EaseFactor, precision: 10);
        Assert.Equal(1, state.Repetitions);
        Assert.Equal(0, state.Lapses);
        Assert.Equal(Now.AddDays(expectedInterval), state.NextReviewAt);
        Assert.Equal(Now, state.LastReviewedAt);
    }

    [Fact]
    public void NewCard_Again_RelearnsSoonWithoutCountingALapse()
    {
        var state = _scheduler.Schedule(NewCard, ReviewRating.Again, Now);

        Assert.Equal(0, state.IntervalDays);
        Assert.Equal(0, state.Repetitions);
        Assert.Equal(0, state.Lapses);
        Assert.Equal(2.3, state.EaseFactor, precision: 10);
        Assert.Equal(Now + Sm2SpacedRepetitionScheduler.RelearnDelay, state.NextReviewAt);
    }

    [Fact]
    public void Good_AfterFirstReview_UsesSecondFixedInterval()
    {
        var state = _scheduler.Schedule(Learned(interval: 1, repetitions: 1), ReviewRating.Good, Now);

        Assert.Equal(6, state.IntervalDays);
        Assert.Equal(2, state.Repetitions);
    }

    [Fact]
    public void Good_AfterSecondReview_MultipliesIntervalByEase()
    {
        var state = _scheduler.Schedule(Learned(interval: 6, repetitions: 2), ReviewRating.Good, Now);

        Assert.Equal(15, state.IntervalDays); // 6 x 2.5
        Assert.Equal(2.5, state.EaseFactor, precision: 10);
        Assert.Equal(3, state.Repetitions);
    }

    [Fact]
    public void ConsecutiveGoodReviews_IncreaseIntervalEachTime()
    {
        var state = NewCard;
        var intervals = new List<int>();
        for (var i = 0; i < 5; i++)
        {
            state = _scheduler.Schedule(state, ReviewRating.Good, state.NextReviewAt);
            intervals.Add(state.IntervalDays);
        }

        Assert.Equal([1, 6, 15, 38, 95], intervals);
    }

    [Fact]
    public void Hard_OnLearnedCard_GrowsSlowlyAndLowersEase()
    {
        var state = _scheduler.Schedule(Learned(interval: 10, repetitions: 2), ReviewRating.Hard, Now);

        Assert.Equal(12, state.IntervalDays); // 10 x 1.2
        Assert.Equal(2.35, state.EaseFactor, precision: 10);
        Assert.Equal(3, state.Repetitions);
    }

    [Fact]
    public void Hard_OnShortInterval_StillGrowsByAtLeastOneDay()
    {
        var state = _scheduler.Schedule(Learned(interval: 1, repetitions: 2), ReviewRating.Hard, Now);

        Assert.Equal(2, state.IntervalDays);
    }

    [Fact]
    public void Easy_OnLearnedCard_AppliesBonusAndRaisesEase()
    {
        var state = _scheduler.Schedule(Learned(interval: 10, repetitions: 2), ReviewRating.Easy, Now);

        Assert.Equal(34, state.IntervalDays); // 10 x 2.65 x 1.3 = 34.45
        Assert.Equal(2.65, state.EaseFactor, precision: 10);
    }

    [Theory]
    [InlineData(1, 1)] // after a first Good/Hard
    [InlineData(4, 1)] // after a first Easy
    [InlineData(6, 2)]
    [InlineData(15, 3)]
    public void Ratings_AreOrdered_HardBelowGoodBelowEasy(int interval, int repetitions)
    {
        var current = Learned(interval, repetitions);

        var hard = _scheduler.Schedule(current, ReviewRating.Hard, Now).IntervalDays;
        var good = _scheduler.Schedule(current, ReviewRating.Good, Now).IntervalDays;
        var easy = _scheduler.Schedule(current, ReviewRating.Easy, Now).IntervalDays;

        Assert.True(hard <= good, $"hard {hard} > good {good}");
        Assert.True(good < easy, $"good {good} >= easy {easy}");
    }

    [Fact]
    public void Again_OnLearnedCard_CountsLapseAndResetsProgress()
    {
        var current = Learned(interval: 15, repetitions: 3, ease: 2.5, lapses: 1);

        var state = _scheduler.Schedule(current, ReviewRating.Again, Now);

        Assert.Equal(2, state.Lapses);
        Assert.Equal(0, state.Repetitions);
        Assert.Equal(0, state.IntervalDays);
        Assert.Equal(2.3, state.EaseFactor, precision: 10);
        Assert.Equal(Now.AddMinutes(10), state.NextReviewAt);
        Assert.Equal(Now, state.LastReviewedAt);
    }

    [Fact]
    public void Good_AfterLapse_StartsAgainFromFirstInterval()
    {
        var lapsed = _scheduler.Schedule(Learned(interval: 15, repetitions: 3), ReviewRating.Again, Now);

        var state = _scheduler.Schedule(lapsed, ReviewRating.Good, lapsed.NextReviewAt);

        Assert.Equal(1, state.IntervalDays);
        Assert.Equal(1, state.Repetitions);
        Assert.Equal(1, state.Lapses);
    }

    [Theory]
    [InlineData(ReviewRating.Again, 1.35)]
    [InlineData(ReviewRating.Hard, 1.3)]
    public void EaseFactor_NeverDropsBelowMinimum(ReviewRating rating, double startingEase)
    {
        var state = _scheduler.Schedule(Learned(interval: 5, repetitions: 3, ease: startingEase), rating, Now);

        Assert.Equal(Sm2SpacedRepetitionScheduler.MinimumEaseFactor, state.EaseFactor, precision: 10);
    }

    [Fact]
    public void RepeatedAgain_KeepsEaseAtMinimum()
    {
        var state = NewCard;
        for (var i = 0; i < 20; i++)
        {
            state = _scheduler.Schedule(state, ReviewRating.Again, Now);
        }

        Assert.Equal(Sm2SpacedRepetitionScheduler.MinimumEaseFactor, state.EaseFactor, precision: 10);
    }

    [Fact]
    public void Interval_IsCappedAtMaximum()
    {
        var state = _scheduler.Schedule(Learned(interval: 30_000, repetitions: 10), ReviewRating.Good, Now);

        Assert.Equal(Sm2SpacedRepetitionScheduler.MaximumIntervalDays, state.IntervalDays);
    }

    [Fact]
    public void NextReviewAt_IsReviewTimePlusInterval()
    {
        var reviewedAt = new DateTime(2026, 3, 1, 23, 30, 0, DateTimeKind.Utc);

        var state = _scheduler.Schedule(Learned(interval: 6, repetitions: 2), ReviewRating.Good, reviewedAt);

        Assert.Equal(new DateTime(2026, 3, 16, 23, 30, 0, DateTimeKind.Utc), state.NextReviewAt);
        Assert.Equal(DateTimeKind.Utc, state.NextReviewAt.Kind);
    }

    [Fact]
    public void Schedule_WithUndefinedRating_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _scheduler.Schedule(NewCard, (ReviewRating)42, Now));
    }
}
