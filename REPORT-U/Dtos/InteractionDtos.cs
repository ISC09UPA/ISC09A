using System.ComponentModel.DataAnnotations;

namespace ReportU.Dtos;

/// <summary>Crear un comentario (solo texto en el MVP, sin respuestas anidadas).</summary>
public class CreateCommentRequest
{
    /// <example>También lo vi esta mañana, ya lo reporté en servicios generales.</example>
    [Required, MaxLength(1000)]
    public string Body { get; set; } = string.Empty;
}

public class CommentResponse
{
    public Guid Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public AuthorInfo Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Estado de apoyo tras PUT/DELETE /api/posts/{id}/support.</summary>
public class SupportResponse
{
    public int SupportCount { get; set; }
    public bool SupportedByMe { get; set; }
}

/// <summary>Estado de guardado tras PUT/DELETE /api/posts/{id}/bookmark.</summary>
public class BookmarkResponse
{
    public bool BookmarkedByMe { get; set; }
}
