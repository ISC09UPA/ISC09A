namespace EasyMemories.Models;

/// <summary>
/// Un recuerdo: una foto (obligatoria) + comentario opcional subido por un invitado.
/// </summary>
public class Memory
{
    public int Id { get; set; }

    public int SpaceId { get; set; }

    public Space? Space { get; set; }

    public string? GuestName { get; set; }

    public string? Comment { get; set; }

    /// <summary>Nombre del blob dentro del contenedor (no la URL, que es firmada y expira).</summary>
    public string BlobName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "image/jpeg";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
