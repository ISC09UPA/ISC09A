namespace KIAN.Models;

public class Card
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ImageBlobName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public List<CardTranslation> Translations { get; set; } = new();
    public List<CardReview> Reviews { get; set; } = new();
}