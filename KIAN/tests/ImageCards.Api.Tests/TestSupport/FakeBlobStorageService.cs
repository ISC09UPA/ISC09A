using System.Collections.Concurrent;
using ImageCards.Api.Exceptions;
using ImageCards.Api.Services.Images;
using ImageCards.Api.Services.Storage;

namespace ImageCards.Api.Tests.TestSupport;

/// <summary>In-memory stand-in for Azure Blob Storage.</summary>
public sealed class FakeBlobStorageService : IBlobStorageService
{
    private readonly ConcurrentDictionary<string, (byte[] Content, string ContentType)> _blobs = new();

    public bool FailUploads { get; set; }

    public bool FailDeletes { get; set; }

    public IReadOnlyCollection<string> StoredBlobNames => _blobs.Keys.ToList();

    public async Task<string> UploadImageAsync(Stream content, ImageFormat format, CancellationToken cancellationToken)
    {
        if (FailUploads)
        {
            throw new StorageUnavailableException("Image storage upload failed.");
        }

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var blobName = BlobNames.Create(format);
        _blobs[blobName] = (buffer.ToArray(), format.ContentType);
        return blobName;
    }

    public Task<bool> DeleteImageAsync(string blobName, CancellationToken cancellationToken)
    {
        if (FailDeletes)
        {
            throw new StorageUnavailableException("Image storage delete failed.");
        }

        return Task.FromResult(_blobs.TryRemove(blobName, out _));
    }

    public Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken) =>
        Task.FromResult(_blobs.ContainsKey(blobName));

    public Uri GetReadUrl(string blobName) => new($"https://fake.blob.local/card-images/{blobName}?sig=test");

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken) => Task.FromResult(true);

    public string ContentTypeOf(string blobName) => _blobs[blobName].ContentType;
}
