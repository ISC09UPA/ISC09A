namespace Closet.Api.Models;

public class Garment
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public BodyPart BodyPart { get; set; }
    public string? FileName { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
