using System.Text;
using Microsoft.AspNetCore.Http;

namespace ImageCards.Api.Tests.TestSupport;

/// <summary>Byte arrays with valid image signatures (the validator only inspects the header).</summary>
public static class TestImages
{
    public static readonly byte[] Png = Pad([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

    public static readonly byte[] Jpeg = Pad([0xFF, 0xD8, 0xFF, 0xE0]);

    public static readonly byte[] Webp = Pad([.. "RIFF"u8, 0x24, 0x00, 0x00, 0x00, .. "WEBP"u8]);

    public static readonly byte[] NotAnImage = Encoding.ASCII.GetBytes("<?php echo 'not an image'; ?>");

    public static IFormFile FormFile(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "image", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };
    }

    public static IFormFile PngFile(string fileName = "apple.png") => FormFile(Png, fileName, "image/png");

    private static byte[] Pad(byte[] header) => [.. header, .. new byte[64]];
}
