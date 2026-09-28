using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RECETASMD.Services
{
    public interface IAzureBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }

    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public AzureBlobStorageService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("El archivo proporcionado está vacío.");
            }

            var connectionString = _configuration["AzureStorage:ConnectionString"];
            var containerName = _configuration["AzureStorage:ContainerName"] ?? "recetas-fotos";

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            // Intentar subir a Azure Blob Storage si hay connection string
            if (!string.IsNullOrWhiteSpace(connectionString) && connectionString != "YOUR_AZURE_CONNECTION_STRING_HERE")
            {
                try
                {
                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                    var blobClient = containerClient.GetBlobClient(fileName);

                    using var stream = file.OpenReadStream();
                    await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

                    return blobClient.Uri.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Azure Storage Warning] No se pudo subir a Azure: {ex.Message}. Usando fallback local.");
                }
            }

            // Fallback Local Storage
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{fileName}";
        }
    }
}
