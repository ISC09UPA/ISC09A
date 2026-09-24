namespace ReportU.Dtos;

/// <summary>Metadato de imagen + URLs para mostrar y descargar.</summary>
public class PostImageResponse
{
    public Guid Id { get; set; }

    /// <summary>URL pública o firmada para mostrar la imagen en la app.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>URL del endpoint de descarga del API: GET /api/media/{id}/download.</summary>
    public string DownloadUrl { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Lista paginada genérica usada por el feed y listados.</summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
