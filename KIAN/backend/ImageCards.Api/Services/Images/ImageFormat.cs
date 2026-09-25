namespace ImageCards.Api.Services.Images;

/// <summary>An accepted image format: MIME type, canonical extension, accepted extensions and binary signature check.</summary>
public sealed class ImageFormat
{
    private readonly Func<ReadOnlyMemory<byte>, bool> _matchesSignature;

    private ImageFormat(
        string contentType,
        string extension,
        string[] acceptedExtensions,
        Func<ReadOnlyMemory<byte>, bool> matchesSignature)
    {
        ContentType = contentType;
        Extension = extension;
        AcceptedExtensions = acceptedExtensions;
        _matchesSignature = matchesSignature;
    }

    /// <summary>Bytes needed from the start of the file to check any signature.</summary>
    public const int SignatureLength = 12;

    public static readonly ImageFormat Jpeg = new(
        "image/jpeg", ".jpg", [".jpg", ".jpeg"],
        header => StartsWith(header, 0, [0xFF, 0xD8, 0xFF]));

    public static readonly ImageFormat Png = new(
        "image/png", ".png", [".png"],
        header => StartsWith(header, 0, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]));

    // RIFF....WEBP
    public static readonly ImageFormat Webp = new(
        "image/webp", ".webp", [".webp"],
        header => StartsWith(header, 0, "RIFF"u8) && StartsWith(header, 8, "WEBP"u8));

    public static readonly IReadOnlyList<ImageFormat> All = [Jpeg, Png, Webp];

    public string ContentType { get; }

    /// <summary>Extension used for the blob name, regardless of the one the client sent.</summary>
    public string Extension { get; }

    public IReadOnlyList<string> AcceptedExtensions { get; }

    public static ImageFormat? FromContentType(string? contentType) =>
        All.FirstOrDefault(f => string.Equals(f.ContentType, contentType, StringComparison.OrdinalIgnoreCase));

    public bool AcceptsExtension(string extension) =>
        AcceptedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);

    public bool MatchesSignature(ReadOnlyMemory<byte> header) => _matchesSignature(header);

    private static bool StartsWith(ReadOnlyMemory<byte> data, int offset, ReadOnlySpan<byte> expected) =>
        data.Length >= offset + expected.Length && data.Span.Slice(offset, expected.Length).SequenceEqual(expected);
}
