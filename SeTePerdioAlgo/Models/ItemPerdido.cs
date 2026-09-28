namespace SeTePerdioAlgo.Models;

public class ItemPerdido
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = "Otro";
    public string Ubicacion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    //Perdido o encontrado
    public string Tipo { get; set; } = "Perdido";

    //Activo o resuelto (que ya se entregó o que ya no está perdido)
    public string Estado { get; set; } = "Activo";

    public string NombreContacto { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;

    public List<FotoItem> Fotos { get; set; } = new();
}
