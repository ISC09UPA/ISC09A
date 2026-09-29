namespace KIAN.Models;

public class CardReview
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;
    public int IntervalDays { get; set; } = 0;
    public decimal EaseFactor { get; set; } = 2.50m;
    public int Repetitions { get; set; } = 0;
    public int Lapses { get; set; } = 0;

    public Card? Card { get; set; }
    public User? User { get; set; }
}