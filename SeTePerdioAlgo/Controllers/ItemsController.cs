using Microsoft.AspNetCore.Mvc;
using SeTePerdioAlgo.DTOs;
using SeTePerdioAlgo.Models;
using SeTePerdioAlgo.Services;

namespace SeTePerdioAlgo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    //Almacenamiento en memoria
    private static readonly List<ItemPerdido> _items = new();
    private static readonly object _lock = new();

    private readonly BlobStorageService _blobStorage;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(BlobStorageService blobStorage, ILogger<ItemsController> logger)
    {
        _blobStorage = blobStorage;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? tipo, [FromQuery] string? categoria, [FromQuery] string? q)
    {
        List<ItemPerdido> copia;
        lock (_lock)
        {
            copia = _items.ToList();
        }

        IEnumerable<ItemPerdido> resultado = copia;

        if (!string.IsNullOrWhiteSpace(tipo))
            resultado = resultado.Where(i => string.Equals(i.Tipo, tipo, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(categoria))
            resultado = resultado.Where(i => string.Equals(i.Categoria, categoria, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(q))
        {
            resultado = resultado.Where(i =>
                i.Titulo.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                i.Descripcion.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                i.Ubicacion.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var respuesta = resultado
            .OrderByDescending(i => i.Fecha)
            .Select(MapToResponse);

        return Ok(respuesta);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item is null) return NotFound(new { mensaje = "Publicación no encontrada" });
        return Ok(MapToResponse(item));
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Create([FromForm] CrearItemRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            return BadRequest(new { mensaje = "El título es obligatorio" });

        if (string.IsNullOrWhiteSpace(request.Ubicacion))
            return BadRequest(new { mensaje = "La ubicación es obligatoria" });

        var tipo = string.Equals(request.Tipo, "Encontrado", StringComparison.OrdinalIgnoreCase)
            ? "Encontrado"
            : "Perdido";

        var item = new ItemPerdido
        {
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            Categoria = string.IsNullOrWhiteSpace(request.Categoria) ? "Otro" : request.Categoria,
            Ubicacion = request.Ubicacion,
            Tipo = tipo,
            NombreContacto = request.NombreContacto,
            Contacto = request.Contacto
        };

        if (request.Fotos is { Count: > 0 })
        {
            foreach (var archivo in request.Fotos.Where(a => a.Length > 0))
            {
                if (!archivo.ContentType.StartsWith("image/"))
                    continue;

                try
                {
                    var subida = await _blobStorage.UploadFotoAsync(archivo, item.Id);
                    item.Fotos.Add(new FotoItem
                    {
                        Url = subida.Url,
                        BlobName = subida.BlobName,
                        EsPrincipal = item.Fotos.Count == 0
                    });
                }
                catch (Exception ex)
                {
                    //No tumbamos toda la publicación si falla una sola foto;
                    //solo la registramos y seguimos con las demás.
                    _logger.LogError(ex, "Error subiendo foto para el item {ItemId}", item.Id);
                }
            }
        }

        lock (_lock)
        {
            _items.Add(item);
        }

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPatch("{id}/estado")]
    public IActionResult ActualizarEstado(string id, [FromBody] ActualizarEstadoRequest request)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item is null) return NotFound(new { mensaje = "Publicación no encontrada" });

        item.Estado = string.Equals(request.Estado, "Resuelto", StringComparison.OrdinalIgnoreCase)
            ? "Resuelto"
            : "Activo";

        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item is null) return NotFound(new { mensaje = "Publicación no encontrada" });

        foreach (var foto in item.Fotos)
            await _blobStorage.DeleteFotoAsync(foto.BlobName);

        lock (_lock)
        {
            _items.Remove(item);
        }

        return Ok(new { mensaje = "Publicación eliminada" });
    }

    private static ItemResponse MapToResponse(ItemPerdido item) => new()
    {
        Id = item.Id,
        Titulo = item.Titulo,
        Descripcion = item.Descripcion,
        Categoria = item.Categoria,
        Ubicacion = item.Ubicacion,
        Fecha = item.Fecha,
        Tipo = item.Tipo,
        Estado = item.Estado,
        NombreContacto = item.NombreContacto,
        Contacto = item.Contacto,
        Fotos = item.Fotos
            .Select(f => new FotoResponse { Id = f.Id, Url = f.Url, EsPrincipal = f.EsPrincipal })
            .ToList()
    };
}
