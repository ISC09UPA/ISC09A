namespace ImageCards.Api.Services.SpacedRepetition;

/// <summary>Scheduler-agnostic snapshot of a card's repetition state. All dates are UTC.</summary>
public sealed record SchedulingState(
    int IntervalDays,
    double EaseFactor,
    int Repetitions,
    int Lapses,
    DateTime NextReviewAt,
    DateTime? LastReviewedAt);
