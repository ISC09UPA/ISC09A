using System.ComponentModel.DataAnnotations;
using ReportU.Models;

namespace ReportU.Dtos;

/// <summary>Autor tal como lo ve el público, ya resuelto según el modo de identidad.</summary>
public class AuthorInfo
{
    /// <example>ana_p</example>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Nulo cuando el autor es anónimo u oculta su matrícula.</summary>
    public string? EnrollmentNumber { get; set; }

    public bool IsAnonymous { get; set; }
}

/// <summary>Creación de una publicación. Las imágenes se suben después con POST /api/posts/{id}/images (máx. 4).</summary>
public class CreatePostRequest
{
    /// <example>Fuga de agua en el edificio C</example>
    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    /// <example>Desde ayer hay una fuga junto a los baños del segundo piso...</example>
    [Required, MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    /// <example>Incidencia</example>
    [Required]
    public PostType Type { get; set; }

    /// <example>Infraestructura</example>
    [Required]
    public PostCategory Category { get; set; }

    /// <example>Edificio C, segundo piso</example>
    [MaxLength(200)]
    public string? Location { get; set; }

    /// <example>Username</example>
    public IdentityMode IdentityMode { get; set; } = IdentityMode.Default;
}

/// <summary>Edición parcial de una publicación propia. Todo es opcional. Solo el autor puede editar.</summary>
public class UpdatePostRequest
{
    [MaxLength(150)]
    public string? Title { get; set; }

    [MaxLength(5000)]
    public string? Description { get; set; }

    public PostType? Type { get; set; }
    public PostCategory? Category { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public IdentityMode? IdentityMode { get; set; }
}

/// <summary>Publicación resumida para el feed (Recientes / Populares) y listados.</summary>
public class PostSummaryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public PostType Type { get; set; }
    public PostCategory Category { get; set; }
    public string? Location { get; set; }
    public AuthorInfo Author { get; set; } = null!;
    public int SupportCount { get; set; }
    public int CommentCount { get; set; }
    public int ImageCount { get; set; }

    /// <summary>Imagen de portada (primera), si existe.</summary>
    public PostImageResponse? CoverImage { get; set; }

    public bool SupportedByMe { get; set; }
    public bool BookmarkedByMe { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Detalle completo de una publicación.</summary>
public class PostDetailResponse : PostSummaryResponse
{
    public string Description { get; set; } = string.Empty;
    public List<PostImageResponse> Images { get; set; } = new();
}
