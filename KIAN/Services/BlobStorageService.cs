using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace KIAN.Services;

public class BlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(IConfiguration config)
    {
        var connectionString = config["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException("Falta AzureStorage:ConnectionString");
        var containerName = config["AzureStorage:ContainerName"] ?? "flashcard-images";

        _container = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        await _container.CreateIfNotExistsAsync(PublicAccessType.None);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var blobName = $"cards/{Guid.NewGuid():N}{extension}";

        var blob = _container.GetBlobClient(blobName);
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

        return blobName;
    }

    public string GetTemporaryUrl(string blobName, int minutes = 30)
    {
        var blob = _container.GetBlobClient(blobName);

        if (!blob.CanGenerateSasUri)
            return string.Empty;

        return blob.GenerateSasUri(BlobSasPermissions.Read,
            DateTimeOffset.UtcNow.AddMinutes(minutes)).ToString();
    }

    public async Task DeleteAsync(string blobName)
    {
        await _container.GetBlobClient(blobName).DeleteIfExistsAsync();
    }
}