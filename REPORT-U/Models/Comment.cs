using System.ComponentModel.DataAnnotations;

namespace ReportU.Models;

/// <summary>Comentario de solo texto sobre una publicación. Sin anidación en el MVP.</summary>
public class Comment
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    [MaxLength(1000)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
