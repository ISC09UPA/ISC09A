namespace SeTePerdioAlgo.Models;

public class Usuario
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
}
