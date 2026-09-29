namespace KIAN.Models;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Card> Cards { get; set; } = new();
    public List<CardReview> Reviews { get; set; } = new();
}