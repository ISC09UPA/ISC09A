using ImageCards.Api.Models;

namespace ImageCards.Api.Services.SpacedRepetition;

/// <summary>
/// Scheduler based on the SM-2 algorithm, adapted to four ratings.
/// <list type="bullet">
/// <item>Again: the card is relearned in <see cref="RelearnDelay"/>, repetitions reset, ease -0.20.
/// A lapse is counted only if the card had been learned (repetitions &gt; 0).</item>
/// <item>Hard: ease -0.15, interval x1.2.</item>
/// <item>Good: ease unchanged, intervals 1, 6, then previous x ease.</item>
/// <item>Easy: ease +0.15, intervals 4, 8, then previous x ease x 1.3.</item>
/// </list>
/// After the first successful review every interval grows by at least one day.
/// Ease never drops below <see cref="MinimumEaseFactor"/>.
/// </summary>
public sealed class Sm2SpacedRepetitionScheduler : ISpacedRepetitionScheduler
{
    public const double DefaultEaseFactor = 2.5;
    public const double MinimumEaseFactor = 1.3;
    public const int MaximumIntervalDays = 36_500;
    public static readonly TimeSpan RelearnDelay = TimeSpan.FromMinutes(10);

    private const double AgainEasePenalty = 0.20;
    private const double HardEasePenalty = 0.15;
    private const double EasyEaseBonus = 0.15;
    private const double HardIntervalMultiplier = 1.2;
    private const double EasyIntervalBonus = 1.3;

    private const int FirstGoodInterval = 1;
    private const int SecondGoodInterval = 6;
    private const int FirstEasyInterval = 4;

    public SchedulingState CreateInitialState(DateTime nowUtc) =>
        new(IntervalDays: 0, DefaultEaseFactor, Repetitions: 0, Lapses: 0, NextReviewAt: nowUtc, LastReviewedAt: null);

    public SchedulingState Schedule(SchedulingState current, ReviewRating rating, DateTime reviewedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (!Enum.IsDefined(rating))
        {
            throw new ArgumentOutOfRangeException(nameof(rating), rating, "Unknown review rating.");
        }

        return rating == ReviewRating.Again
            ? Relearn(current, reviewedAtUtc)
            : Advance(current, rating, reviewedAtUtc);
    }

    private static SchedulingState Relearn(SchedulingState current, DateTime reviewedAtUtc) =>
        new(
            IntervalDays: 0,
            EaseFactor: ClampEase(current.EaseFactor - AgainEasePenalty),
            Repetitions: 0,
            Lapses: current.Repetitions > 0 ? current.Lapses + 1 : current.Lapses,
            NextReviewAt: reviewedAtUtc.Add(RelearnDelay),
            LastReviewedAt: reviewedAtUtc);

    private static SchedulingState Advance(SchedulingState current, ReviewRating rating, DateTime reviewedAtUtc)
    {
        var ease = rating switch
        {
            ReviewRating.Hard => ClampEase(current.EaseFactor - HardEasePenalty),
            ReviewRating.Easy => ClampEase(current.EaseFactor + EasyEaseBonus),
            _ => current.EaseFactor,
        };
        var interval = NextInterval(current, rating, ease);

        return new(
            IntervalDays: interval,
            EaseFactor: ease,
            Repetitions: current.Repetitions + 1,
            Lapses: current.Lapses,
            NextReviewAt: reviewedAtUtc.AddDays(interval),
            LastReviewedAt: reviewedAtUtc);
    }

    private static int NextInterval(SchedulingState current, ReviewRating rating, double ease)
    {
        if (current.Repetitions == 0)
        {
            return rating == ReviewRating.Easy ? FirstEasyInterval : FirstGoodInterval;
        }

        var previous = Math.Max(current.IntervalDays, 1);
        var raw = (current.Repetitions, rating) switch
        {
            (_, ReviewRating.Hard) => previous * HardIntervalMultiplier,
            (1, ReviewRating.Good) => SecondGoodInterval,
            (1, ReviewRating.Easy) => SecondGoodInterval * EasyIntervalBonus,
            (_, ReviewRating.Good) => previous * ease,
            _ => previous * ease * EasyIntervalBonus,
        };

        var interval = Math.Max(previous + 1, (int)Math.Round(raw, MidpointRounding.AwayFromZero));
        return Math.Min(interval, MaximumIntervalDays);
    }

    // Rounded to avoid floating point drift accumulating across many reviews.
    private static double ClampEase(double ease) => Math.Round(Math.Max(MinimumEaseFactor, ease), 2);
}
