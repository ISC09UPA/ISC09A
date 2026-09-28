using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using SeTePerdioAlgo.DTOs;
using SeTePerdioAlgo.Models;

namespace SeTePerdioAlgo.Services;

public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        BlobServiceClient blobServiceClient,
        IConfiguration configuration,
        ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = configuration["AzureBlobStorage:ContainerName"]
            ?? throw new ArgumentNullException(nameof(configuration), "Container name not configured");
        _logger = logger;
    }

    public async Task<UploadResponse> UploadFotoAsync(IFormFile archivo, string itemId)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobName = $"{itemId}/{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
        var blobClient = containerClient.GetBlobClient(blobName);

        await using var stream = archivo.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobHttpHeaders
        {
            ContentType = archivo.ContentType
        });

        _logger.LogInformation("Foto subida: {BlobName}", blobName);

        return new UploadResponse
        {
            BlobName = blobName,
            Url = GenerarUrlSas(blobClient)
        };
    }

    private string GenerarUrlSas(BlobClient blobClient)
    {
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    public async Task<List<FotoItem>> UploadMultipleFotosAsync(
        List<IFormFile> archivos, string itemId)
    {
        var fotos = new List<FotoItem>();

        foreach (var archivo in archivos)
        {
            var response = await UploadFotoAsync(archivo, itemId);
            fotos.Add(new FotoItem
            {
                BlobName = response.BlobName,
                Url = response.Url,
                EsPrincipal = fotos.Count == 0
            });
        }

        return fotos;
    }

    public async Task<bool> DeleteFotoAsync(string blobName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
            _logger.LogInformation("Foto eliminada: {BlobName}", blobName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando foto: {BlobName}", blobName);
            return false;
        }
    }

    public async Task<List<string>> ListFotosByItemAsync(string itemId)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var urls = new List<string>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: $"{itemId}/"))
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            urls.Add(GenerarUrlSas(blobClient));
        }

        return urls;
    }
}
