using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using EasyMemories.Options;
using Microsoft.Extensions.Options;

namespace EasyMemories.Services;

/// <summary>
/// La cuenta de Storage tiene "Blob anonymous access" deshabilitado (buena práctica de seguridad),
/// así que las fotos nunca son públicas por URL directa: cada lectura se sirve con un SAS
/// de tiempo limitado generado bajo demanda.
/// </summary>
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;
    private readonly AzureBlobOptions _options;

    public BlobStorageService(IOptions<AzureBlobOptions> options)
    {
        _options = options.Value;
        var serviceClient = new BlobServiceClient(_options.ConnectionString);
        _container = serviceClient.GetBlobContainerClient(_options.ContainerName);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task<string> UploadPhotoAsync(Stream content, string contentType, CancellationToken ct = default)
    {
        var blobName = $"{Guid.NewGuid():N}{ExtensionFor(contentType)}";
        var blobClient = _container.GetBlobClient(blobName);

        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, ct);

        return blobName;
    }

    public string GetReadUrl(string blobName)
    {
        var blobClient = _container.GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException(
                "No se puede generar SAS: la conexión debe usar una cuenta compartida (connection string con clave), no un token SAS ya delegado.");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_options.SasExpiryMinutes)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    public async Task DeleteAsync(string blobName, CancellationToken ct = default)
    {
        await _container.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: ct);
    }

    private static string ExtensionFor(string contentType) => contentType switch
    {
        "image/png" => ".png",
        "image/webp" => ".webp",
        "image/heic" => ".heic",
        _ => ".jpg"
    };
}
