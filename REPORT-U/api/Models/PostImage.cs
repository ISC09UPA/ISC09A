using System.ComponentModel.DataAnnotations;

namespace ReportU.Models;

/// <summary>Metadato de una imagen (0–4 por publicación) cuyo binario vive en Azure Blob Storage.</summary>
public class PostImage
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    /// <summary>Nombre del blob dentro del contenedor, ej. "{postId}/{imageId}.jpg".</summary>
    [MaxLength(500)]
    public string BlobName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTime CreatedAt { get; set; }
}
