using System.Text.RegularExpressions;
using ImageCards.Api.Services.Images;

namespace ImageCards.Api.Services.Storage;

/// <summary>
/// Generates and validates blob names. Names are a random GUID plus a known extension,
/// so they can never contain paths, "..", or anything taken from the client.
/// </summary>
public static partial class BlobNames
{
    public static string Create(ImageFormat format) => $"{Guid.NewGuid():N}{format.Extension}";

    public static bool IsValid(string? blobName) => blobName is not null && BlobNamePattern().IsMatch(blobName);

    public static void EnsureValid(string blobName)
    {
        if (!IsValid(blobName))
        {
            throw new ArgumentException("Invalid blob name.", nameof(blobName));
        }
    }

    [GeneratedRegex(@"^[0-9a-f]{32}\.(jpg|png|webp)$", RegexOptions.CultureInvariant)]
    private static partial Regex BlobNamePattern();
}
