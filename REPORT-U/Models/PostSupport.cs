namespace ReportU.Models;

/// <summary>Apoyo de un estudiante a una publicación. Regla MVP: 1 usuario + 1 publicación = máximo 1 apoyo.</summary>
public class PostSupport
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
