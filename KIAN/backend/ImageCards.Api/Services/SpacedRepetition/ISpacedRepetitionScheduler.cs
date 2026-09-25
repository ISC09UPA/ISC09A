using ImageCards.Api.Models;

namespace ImageCards.Api.Services.SpacedRepetition;

/// <summary>
/// Computes when a card should be reviewed again. Implementations must be pure and deterministic:
/// no database, no system clock. The caller passes the current time.
/// Current implementation: <see cref="Sm2SpacedRepetitionScheduler"/>. An FSRS scheduler can be
/// registered instead without changing callers.
/// </summary>
public interface ISpacedRepetitionScheduler
{
    /// <summary>State of a card that was never reviewed. It is due immediately.</summary>
    SchedulingState CreateInitialState(DateTime nowUtc);

    SchedulingState Schedule(SchedulingState current, ReviewRating rating, DateTime reviewedAtUtc);
}
