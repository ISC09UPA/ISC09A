namespace SeTePerdioAlgo.DTOs;

public class CrearItemRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = "Otro";
    public string Ubicacion { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Perdido";
    public string NombreContacto { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public List<IFormFile>? Fotos { get; set; }
}

public class ActualizarEstadoRequest
{
    public string Estado { get; set; } = "Resuelto";
}

public class FotoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
}

public class ItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string NombreContacto { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public List<FotoResponse> Fotos { get; set; } = new();
}
