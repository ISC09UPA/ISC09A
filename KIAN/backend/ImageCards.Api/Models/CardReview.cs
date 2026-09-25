namespace ImageCards.Api.Models;

/// <summary>Spaced repetition state of a card for a given user.</summary>
public class CardReview
{
    public Guid Id { get; set; }

    public Guid CardId { get; set; }

    public Card? Card { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public DateTime NextReviewAt { get; set; }

    /// <summary>Null until the first review. Kept because schedulers such as FSRS need the elapsed time.</summary>
    public DateTime? LastReviewedAt { get; set; }

    public int IntervalDays { get; set; }

    public double EaseFactor { get; set; }

    public int Repetitions { get; set; }

    public int Lapses { get; set; }
}
