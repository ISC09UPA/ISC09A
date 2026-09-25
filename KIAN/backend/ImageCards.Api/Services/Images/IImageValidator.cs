namespace ImageCards.Api.Services.Images;

public interface IImageValidator
{
    /// <summary>
    /// Validates size, MIME type, extension and file signature.
    /// Throws <see cref="Exceptions.RequestValidationException"/> when the file is not an accepted image.
    /// </summary>
    Task<ImageFormat> ValidateAsync(IFormFile file, string fieldName, CancellationToken cancellationToken);
}
