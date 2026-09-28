namespace EasyMemories.Services;

public interface IBlobStorageService
{
    /// <summary>Sube una foto y devuelve el nombre del blob generado (no la URL).</summary>
    Task<string> UploadPhotoAsync(Stream content, string contentType, CancellationToken ct = default);

    /// <summary>Genera una URL de lectura firmada (SAS) y con expiración para un blob existente.</summary>
    string GetReadUrl(string blobName);

    Task DeleteAsync(string blobName, CancellationToken ct = default);
}
