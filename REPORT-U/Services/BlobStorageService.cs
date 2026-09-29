using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using ReportU.Configuration;

namespace ReportU.Services;

/// <summary>
/// Abstracción del almacenamiento de binarios de imágenes.
/// Producción: Azure Blob Storage. Desarrollo sin credenciales: carpeta local.
/// El contrato (blobName, stream, contentType) es idéntico en ambos casos.
/// </summary>
public interface IBlobStorageService
{
    bool IsCloud { get; }
    Task UploadAsync(string blobName, Stream content, string contentType, CancellationToken ct = default);

    /// <summary>Lista los nombres de blobs cuyo nombre comienza con <paramref name="prefix"/>. Vacío = todos.</summary>
    Task<IReadOnlyList<string>> ListAsync(string? prefix = null, CancellationToken ct = default);

    /// <summary>Genera una URL de solo lectura para el blob. En Azure devuelve un SAS token temporal; en local devuelve la ruta relativa.</summary>
    Task<string> GetReadUrlAsync(string blobName, TimeSpan? expiry = null, CancellationToken ct = default);

    Task<(Stream Content, string ContentType, long Length)> OpenReadAsync(string blobName, CancellationToken ct = default);
    Task DeleteAsync(string blobName, CancellationToken ct = default);
}

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;

    public bool IsCloud => true;

    public AzureBlobStorageService(IOptions<BlobOptions> options)
    {
        var o = options.Value;
        _container = new BlobContainerClient(o.ConnectionString, o.Container);
    }

    public async Task EnsureContainerAsync(CancellationToken ct = default)
    {
        await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);
    }

    public async Task UploadAsync(string blobName, Stream content, string contentType, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(blobName);
        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListAsync(string? prefix = null, CancellationToken ct = default)
    {
        var names = new List<string>();
        await foreach (var item in _container.GetBlobsAsync(prefix: prefix, cancellationToken: ct))
        {
            names.Add(item.Name);
        }
        return names;
    }

    /// <inheritdoc />
    public Task<string> GetReadUrlAsync(string blobName, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(blobName);

        if (!blob.CanGenerateSasUri)
            throw new InvalidOperationException(
                "El BlobClient no puede generar SAS URIs. Verifica que la ConnectionString incluya la account key.");

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = blobName,
            Resource = "b", // b = blob
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromHours(1)),
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blob.GenerateSasUri(sasBuilder);
        return Task.FromResult(sasUri.AbsoluteUri);
    }

    public async Task<(Stream Content, string ContentType, long Length)> OpenReadAsync(string blobName, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(blobName);
        try
        {
            var download = await blob.DownloadStreamingAsync(cancellationToken: ct);
            return (download.Value.Content, download.Value.Details.ContentType, download.Value.Details.ContentLength);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            throw new FileNotFoundException($"Blob '{blobName}' no encontrado.", ex);
        }
    }

    public async Task DeleteAsync(string blobName, CancellationToken ct = default)
    {
        await _container.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: ct);
    }
}

/// <summary>Fallback local para desarrollar sin credenciales de Azure. No usar en producción.</summary>
public class LocalFileStorageService : IBlobStorageService
{
    private readonly string _root;
    public bool IsCloud => false;

    public LocalFileStorageService(IOptions<BlobOptions> options, IWebHostEnvironment env)
    {
        var configured = options.Value.LocalPath;
        _root = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(env.ContentRootPath, configured);
        Directory.CreateDirectory(_root);
    }

    private string PathFor(string blobName)
    {
        var full = Path.GetFullPath(Path.Combine(_root, blobName.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(Path.GetFullPath(_root)))
            throw new ArgumentException("Nombre de blob inválido.", nameof(blobName));
        return full;
    }

    public async Task UploadAsync(string blobName, Stream content, string contentType, CancellationToken ct = default)
    {
        var path = PathFor(blobName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var fs = File.Create(path);
        await content.CopyToAsync(fs, ct);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> ListAsync(string? prefix = null, CancellationToken ct = default)
    {
        if (!Directory.Exists(_root))
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

        var files = Directory.GetFiles(_root, "*.*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(_root, f).Replace(Path.DirectorySeparatorChar, '/'))
            .Where(name => string.IsNullOrEmpty(prefix) || name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Order()
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(files);
    }

    /// <inheritdoc />
    public Task<string> GetReadUrlAsync(string blobName, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var path = PathFor(blobName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Blob '{blobName}' no encontrado.");

        // En desarrollo local devolvemos la ruta relativa al storage.
        var relativePath = Path.GetRelativePath(_root, path).Replace(Path.DirectorySeparatorChar, '/');
        return Task.FromResult($"/uploads/{relativePath}");
    }

    public Task<(Stream Content, string ContentType, long Length)> OpenReadAsync(string blobName, CancellationToken ct = default)
    {
        var path = PathFor(blobName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Blob '{blobName}' no encontrado.");
        var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = BlobContentTypes.FromExtension(Path.GetExtension(path));
        return Task.FromResult<(Stream, string, long)>((fs, contentType, fs.Length));
    }

    public Task DeleteAsync(string blobName, CancellationToken ct = default)
    {
        var path = PathFor(blobName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
}

internal static class BlobContentTypes
{
    public static string FromExtension(string ext) => ext.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        _ => "application/octet-stream",
    };
}
