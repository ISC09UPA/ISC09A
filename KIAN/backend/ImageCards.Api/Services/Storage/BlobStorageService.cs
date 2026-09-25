using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ImageCards.Api.Configuration;
using ImageCards.Api.Exceptions;
using ImageCards.Api.Services.Images;
using Microsoft.Extensions.Options;

namespace ImageCards.Api.Services.Storage;

/// <summary>
/// Azure Blob Storage implementation. The container is private; clients only get short-lived read SAS URLs.
/// Registered as a singleton because <see cref="BlobContainerClient"/> is thread-safe.
/// </summary>
public sealed class BlobStorageService : IBlobStorageService
{
    // Tolerates small clock differences between this server and Azure.
    private static readonly TimeSpan SasClockSkew = TimeSpan.FromMinutes(5);

    private readonly BlobContainerClient _container;
    private readonly TimeSpan _sasLifetime;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly SemaphoreSlim _containerInitLock = new(1, 1);
    private volatile bool _containerReady;

    public BlobStorageService(
        IOptions<AzureStorageOptions> options,
        TimeProvider timeProvider,
        ILogger<BlobStorageService> logger)
    {
        var settings = options.Value;
        _container = new BlobContainerClient(settings.ConnectionString, settings.ContainerName);
        _sasLifetime = TimeSpan.FromMinutes(settings.SasExpiryMinutes);
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(Stream content, ImageFormat format, CancellationToken cancellationToken)
    {
        var blobName = BlobNames.Create(format);
        try
        {
            await EnsureContainerAsync(cancellationToken);
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = format.ContentType },
                // Never overwrite: a GUID collision must fail instead of replacing another card's image.
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All },
            };
            await _container.GetBlobClient(blobName).UploadAsync(content, uploadOptions, cancellationToken);
            _logger.LogInformation("Uploaded image blob {BlobName} ({ContentType})", blobName, format.ContentType);
            return blobName;
        }
        catch (RequestFailedException ex)
        {
            throw StorageFailure(ex, "upload", blobName);
        }
    }

    public async Task<bool> DeleteImageAsync(string blobName, CancellationToken cancellationToken)
    {
        BlobNames.EnsureValid(blobName);
        try
        {
            var response = await _container.GetBlobClient(blobName)
                .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
            _logger.LogInformation("Deleted image blob {BlobName} (existed: {Existed})", blobName, response.Value);
            return response.Value;
        }
        catch (RequestFailedException ex)
        {
            throw StorageFailure(ex, "delete", blobName);
        }
    }

    public async Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken)
    {
        BlobNames.EnsureValid(blobName);
        try
        {
            var response = await _container.GetBlobClient(blobName).ExistsAsync(cancellationToken);
            return response.Value;
        }
        catch (RequestFailedException ex)
        {
            throw StorageFailure(ex, "check", blobName);
        }
    }

    public Uri GetReadUrl(string blobName)
    {
        BlobNames.EnsureValid(blobName);
        var blob = _container.GetBlobClient(blobName);
        if (!blob.CanGenerateSasUri)
        {
            // Happens when the connection string has no account key (e.g. SAS-only or token credentials).
            throw new StorageUnavailableException("Blob storage is not configured to generate read URLs.");
        }

        var now = _timeProvider.GetUtcNow();
        var sas = new BlobSasBuilder(BlobSasPermissions.Read, now.Add(_sasLifetime))
        {
            BlobContainerName = _container.Name,
            BlobName = blobName,
            Resource = "b",
            StartsOn = now.Subtract(SasClockSkew),
            Protocol = _container.Uri.Scheme == Uri.UriSchemeHttps ? SasProtocol.Https : SasProtocol.HttpsAndHttp,
        };
        return blob.GenerateSasUri(sas);
    }

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _container.GetPropertiesAsync(cancellationToken: cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == StatusCodes.Status404NotFound)
        {
            // Reachable; the container is created on first upload.
            return true;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogWarning("Blob storage health check failed with status {Status} ({ErrorCode})", ex.Status, ex.ErrorCode);
            return false;
        }
    }

    private async Task EnsureContainerAsync(CancellationToken cancellationToken)
    {
        if (_containerReady)
        {
            return;
        }

        await _containerInitLock.WaitAsync(cancellationToken);
        try
        {
            if (!_containerReady)
            {
                await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
                _containerReady = true;
            }
        }
        finally
        {
            _containerInitLock.Release();
        }
    }

    private StorageUnavailableException StorageFailure(RequestFailedException ex, string operation, string blobName)
    {
        // Log status and error code only: exception messages may contain request URLs.
        _logger.LogError(
            "Blob storage {Operation} failed for {BlobName} with status {Status} ({ErrorCode})",
            operation, blobName, ex.Status, ex.ErrorCode);
        return new StorageUnavailableException($"Image storage {operation} failed.", ex);
    }
}
