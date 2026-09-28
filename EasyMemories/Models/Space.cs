namespace EasyMemories.Models;

/// <summary>
/// Un "espacio" es el evento/fiesta creado por el admin.
/// El JoinCode es el único secreto que protege el espacio: quien lo conoce (via QR) puede
/// ver y subir recuerdos, no hay login de invitado.
/// </summary>
public class Space
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string JoinCode { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string OwnerUserId { get; set; } = string.Empty;

    public AppUser? Owner { get; set; }

    public ICollection<Memory> Memories { get; set; } = new List<Memory>();
}
