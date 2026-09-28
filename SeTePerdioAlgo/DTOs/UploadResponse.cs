namespace SeTePerdioAlgo.DTOs;

public class UploadResponse
{
    public string BlobName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Message { get; set; } = "Foto subida correctamente";
}
