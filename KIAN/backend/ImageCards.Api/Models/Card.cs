namespace ImageCards.Api.Models;

public class Card
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    /// <summary>Name of the image blob in Azure Blob Storage. The image itself is never stored in SQL.</summary>
    public required string ImageBlobName { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<CardTranslation> Translations { get; set; } = [];

    public ICollection<CardReview> Reviews { get; set; } = [];
}
