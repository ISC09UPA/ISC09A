namespace Closet.Api.Services;

public static class ImageSignature
{
    public static async Task<(string Extension, string ContentType)?> DetectAsync(
        Stream stream, CancellationToken ct = default)
    {
        var h = new byte[12];
        var read = 0;
        while (read < h.Length)
        {
            var n = await stream.ReadAsync(h.AsMemory(read), ct);
            if (n == 0) break;
            read += n;
        }

        if (read >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF)
            return (".jpg", "image/jpeg");

        if (read >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47
            && h[4] == 0x0D && h[5] == 0x0A && h[6] == 0x1A && h[7] == 0x0A)
            return (".png", "image/png");

        if (read >= 12 && h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46
            && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50)
            return (".webp", "image/webp");

        return null;
    }
}