using SeTePerdioAlgo.DTOs;
using SeTePerdioAlgo.Services;
using Microsoft.AspNetCore.Mvc;

namespace SeTePerdioAlgo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FotosController : ControllerBase
{
    private readonly BlobStorageService _blobStorage;
    private readonly ILogger<FotosController> _logger;

    public FotosController(BlobStorageService blobStorage, ILogger<FotosController> logger)
    {
        _blobStorage = blobStorage;
        _logger = logger;
    }

    [HttpPost("upload/{itemId}")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Upload(
        string itemId,
        IFormFile archivo,
        CancellationToken cancellationToken)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { mensaje = "No se recibio ninguna foto" });

        if (!archivo.ContentType.StartsWith("image/"))
            return BadRequest(new { mensaje = "Solo se permiten archivos de imagen" });

        try
        {
            var response = await _blobStorage.UploadFotoAsync(archivo, itemId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subiendo foto");
            return StatusCode(500, new { mensaje = "Error interno al subir la foto" });
        }
    }

    [HttpPost("upload-multiple/{itemId}")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadMultiple(
        string itemId,
        List<IFormFile> archivos,
        CancellationToken cancellationToken)
    {
        if (archivos is null || archivos.Count == 0)
            return BadRequest(new { mensaje = "No se recibieron fotos" });

        var invalidFiles = archivos.Where(a => !a.ContentType.StartsWith("image/")).ToList();
        if (invalidFiles.Any())
            return BadRequest(new { mensaje = "Solo se permiten archivos de imagen" });

        try
        {
            var fotos = await _blobStorage.UploadMultipleFotosAsync(archivos, itemId);
            return Ok(new { fotos, total = fotos.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subiendo multiples fotos");
            return StatusCode(500, new { mensaje = "Error interno al subir las fotos" });
        }
    }

    [HttpDelete("{blobName}")]
    public async Task<IActionResult> Delete(string blobName)
    {
        var deleted = await _blobStorage.DeleteFotoAsync(blobName);
        if (!deleted)
            return NotFound(new { mensaje = "Foto no encontrada" });

        return Ok(new { mensaje = "Foto eliminada correctamente" });
    }

    [HttpGet("item/{itemId}")]
    public async Task<IActionResult> GetByItem(string itemId)
    {
        var urls = await _blobStorage.ListFotosByItemAsync(itemId);
        return Ok(new { itemId, fotos = urls, total = urls.Count });
    }
}
