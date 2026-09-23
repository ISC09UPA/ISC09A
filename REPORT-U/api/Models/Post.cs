using System.ComponentModel.DataAnnotations;

namespace ReportU.Models;

/// <summary>Publicación de un estudiante (incidencia, queja o discusión).</summary>
public class Post
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    public PostType Type { get; set; }
    public PostCategory Category { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public IdentityMode IdentityMode { get; set; } = IdentityMode.Default;

    /// <summary>Contador desnormalizado para ordenar el feed "Populares".</summary>
    public int SupportCount { get; set; }

    public int CommentCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostSupport> Supports { get; set; } = new List<PostSupport>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
