using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportU.Dtos;
using ReportU.Services;

namespace ReportU.Controllers;

/// <summary>
/// Imágenes de publicaciones: subir, listar, mostrar, descargar y eliminar.
/// Controlador delgado: toda la orquestación EF Core + Blob Storage vive en
/// <see cref="IPostImageService"/>. Los errores de dominio salen como
/// ProblemDetails con su <c>code</c> vía <c>PostImageException</c>.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public class MediaController(
    IPostImageService images,
    ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>Sube una imagen a una publicación propia (repetir hasta 4). El binario va al Blob Storage; aquí se guarda su metadato.</summary>
    /// <param name="postId">Id de la publicación (debe ser tuya).</param>
    /// <param name="file">Imagen (jpg, png, webp o gif, máx. 5 MB) como multipart/form-data.</param>
    /// <response code="201">Imagen subida. `url` sirve para mostrarla y `downloadUrl` para descargarla.</response>
    /// <response code="400">Archivo ausente o vacío.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="403">No eres el autor (`forbidden_not_owner`).</response>
    /// <response code="404">La publicación no existe.</response>
    /// <response code="409">La publicación ya tiene 4 imágenes (`image_limit_reached`).</response>
    /// <response code="413">La imagen supera 5 MB (`payload_too_large`).</response>
    /// <response code="415">Tipo de archivo no permitido (`unsupported_media_type`).</response>
    [HttpPost("posts/{postId:guid}/images")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PostImageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<PostImageResponse>> UploadImage([FromRoute] Guid postId, IFormFile? file)
    {
        if (file is null)
            throw PostImageException.EmptyFile();

        await using var stream = file.OpenReadStream();
        var image = await images.UploadAsync(
            postId, currentUser.UserId!.Value, stream, file.ContentType, file.Length);

        return CreatedAtAction(
            nameof(DownloadImage), new { imageId = image.Id }, PostsController.ToImage(image));
    }

    /// <summary>Lista las imágenes (metadatos + URLs) de una publicación.</summary>
    /// <param name="postId">Id de la publicación.</param>
    /// <response code="200">Lista de imágenes.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">La publicación no existe.</response>
    [HttpGet("posts/{postId:guid}/images")]
    [ProducesResponseType(typeof(List<PostImageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PostImageResponse>>> ListImages([FromRoute] Guid postId)
    {
        var list = await images.ListAsync(postId);
        return Ok(list.Select(PostsController.ToImage).ToList());
    }

    /// <summary>Muestra el binario de una imagen (para el visor de imágenes de la app). Responde con el Content-Type original.</summary>
    /// <param name="imageId">Id de la imagen.</param>
    /// <response code="200">Binario de la imagen.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">La imagen no existe (ni en BD ni en el blob).</response>
    [HttpGet("media/{imageId:guid}")]
    [Produces("image/jpeg", "image/png", "image/webp", "image/gif")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShowImage([FromRoute] Guid imageId)
    {
        var (content, contentType, _) = await images.OpenReadAsync(imageId);
        return File(content, contentType);
    }

    /// <summary>Descarga el archivo original de una imagen (Content-Disposition: attachment).</summary>
    /// <param name="imageId">Id de la imagen.</param>
    /// <response code="200">Archivo descargado.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">La imagen no existe (ni en BD ni en el blob).</response>
    [HttpGet("media/{imageId:guid}/download")]
    [Produces("image/jpeg", "image/png", "image/webp", "image/gif")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadImage([FromRoute] Guid imageId)
    {
        var (content, contentType, _) = await images.OpenReadAsync(imageId);
        return File(content, contentType, $"{imageId:N}{ExtensionFor(contentType)}");
    }

    /// <summary>Elimina una imagen de tu publicación (borra el metadato y el blob). Solo el autor.</summary>
    /// <param name="imageId">Id de la imagen.</param>
    /// <response code="204">Imagen eliminada del API y del Blob Storage.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="403">No eres el autor (`forbidden_not_owner`).</response>
    /// <response code="404">La imagen no existe.</response>
    /// <response code="502">La imagen se borró de la BD pero el blob falló (`blob_error`, reintentar borrado).</response>
    [HttpDelete("media/{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage([FromRoute] Guid imageId)
    {
        await images.DeleteAsync(imageId, currentUser.UserId!.Value);
        return NoContent();
    }

    private static string ExtensionFor(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        "image/gif" => ".gif",
        _ => string.Empty,
    };
}
