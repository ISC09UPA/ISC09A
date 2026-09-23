namespace ReportU.Models;

/// <summary>Publicación guardada por un estudiante para su pantalla "Guardados".</summary>
public class Bookmark
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
