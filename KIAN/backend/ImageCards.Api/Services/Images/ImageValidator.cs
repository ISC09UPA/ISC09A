using ImageCards.Api.Configuration;
using ImageCards.Api.Exceptions;
using Microsoft.Extensions.Options;

namespace ImageCards.Api.Services.Images;

public sealed class ImageValidator(IOptions<ImageUploadOptions> options) : IImageValidator
{
    private readonly long _maxBytes = options.Value.MaxBytes;

    public async Task<ImageFormat> ValidateAsync(IFormFile file, string fieldName, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            throw new RequestValidationException(fieldName, "The image is empty.");
        }

        if (file.Length > _maxBytes)
        {
            throw new RequestValidationException(fieldName, $"The image exceeds the maximum size of {_maxBytes} bytes.");
        }

        var format = ImageFormat.FromContentType(file.ContentType)
            ?? throw new RequestValidationException(
                fieldName,
                $"Unsupported content type. Allowed: {string.Join(", ", ImageFormat.All.Select(f => f.ContentType))}.");

        // Only the extension of the client file name is used, and only for validation.
        // The file name itself is never used to build the blob name.
        var extension = Path.GetExtension(Path.GetFileName(file.FileName));
        if (!format.AcceptsExtension(extension))
        {
            throw new RequestValidationException(fieldName, "The file extension does not match the content type.");
        }

        var header = await ReadHeaderAsync(file, cancellationToken);
        if (!format.MatchesSignature(header))
        {
            throw new RequestValidationException(fieldName, "The file content is not a valid image of the declared type.");
        }

        return format;
    }

    private static async Task<ReadOnlyMemory<byte>> ReadHeaderAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var buffer = new byte[ImageFormat.SignatureLength];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false, cancellationToken);
        return buffer.AsMemory(0, read);
    }
}
