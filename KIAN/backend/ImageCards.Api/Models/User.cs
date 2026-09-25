namespace ImageCards.Api.Models;

public class User
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Card> Cards { get; set; } = [];

    public ICollection<CardReview> CardReviews { get; set; } = [];
}
