using System.ComponentModel.DataAnnotations;

namespace ImageCards.Api.Configuration;

public class ImageUploadOptions
{
    public const string SectionName = "ImageUpload";

    public const long DefaultMaxBytes = 5 * 1024 * 1024;

    [Range(1, 20 * 1024 * 1024)]
    public long MaxBytes { get; set; } = DefaultMaxBytes;
}
