using ImageCards.Api.Services.Images;

namespace ImageCards.Api.Services.Storage;

/// <summary>Stores card images. Blob names are generated here; callers never choose them.</summary>
public interface IBlobStorageService
{
    /// <summary>Uploads an already validated image and returns the generated blob name.</summary>
    Task<string> UploadImageAsync(Stream content, ImageFormat format, CancellationToken cancellationToken);

    /// <summary>Deletes the blob if it exists. Returns false when it did not exist.</summary>
    Task<bool> DeleteImageAsync(string blobName, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken);

    /// <summary>Returns a temporary read-only URL (SAS) for the blob.</summary>
    Uri GetReadUrl(string blobName);

    /// <summary>Used by the health check.</summary>
    Task<bool> CanConnectAsync(CancellationToken cancellationToken);
}
