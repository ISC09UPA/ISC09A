namespace SeTePerdioAlgo.Models;

public class FotoItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Url { get; set; } = string.Empty;
    public string BlobName { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
}
