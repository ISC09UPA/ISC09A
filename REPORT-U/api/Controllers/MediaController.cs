using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportU.Data;
using ReportU.Dtos;
using ReportU.Models;
using ReportU.Services;

namespace ReportU.Controllers;

/// <summary>Imágenes de publicaciones: subir, listar, mostrar, descargar y eliminar (binarios en Azure Blob Storage).</summary>
[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public class MediaController(
    ReportUDbContext db,
    ICurrentUserService currentUser,
    IBlobStorageService blobs) : ControllerBase
{
    /// <summary>Máximo de imágenes por publicación según el MVP.</summary>
    public const int MaxImagesPerPost = 4;

    /// <summary>Tamaño máximo por imagen: 5 MB.</summary>
    public const long MaxImageBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif",
    };

    private static readonly Dictionary<string, string> ExtensionByContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif",
    };

    /// <summary>Sube una imagen a una publicación propia (repetir hasta 4). El binario va a Azure Blob Storage; aquí se guarda su metadato.</summary>
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
    public async Task<ActionResult<PostImageResponse>> UploadImage([FromRoute] Guid postId, IFormFile file)
    {
        var post = await db.Posts.Include(p => p.Images).SingleOrDefaultAsync(p => p.Id == postId);
        if (post is null) return PostNotFound();
        if (post.AuthorId != currentUser.UserId) return NotOwner();

        if (file is null || file.Length == 0)
            return BadRequest(SimpleProblem(400, "Archivo vacío", "Debes enviar una imagen como multipart/form-data.", Middleware.ErrorCodes.ValidationError));
        if (!AllowedContentTypes.Contains(file.ContentType))
            return StatusCode(415, SimpleProblem(415, "Tipo no permitido", "Solo se permiten imágenes jpg, png, webp o gif.", Middleware.ErrorCodes.UnsupportedMediaType));
        if (file.Length > MaxImageBytes)
            return StatusCode(413, SimpleProblem(413, "Imagen muy grande", "Cada imagen debe pesar máximo 5 MB.", Middleware.ErrorCodes.PayloadTooLarge));
        if (post.Images.Count >= MaxImagesPerPost)
            return Conflict(SimpleProblem(409, "Límite de imágenes", "Una publicación puede tener máximo 4 imágenes.", Middleware.ErrorCodes.ImageLimitReached));

        var imageId = Guid.NewGuid();
        var blobName = $"{postId:N}/{imageId:N}{ExtensionByContentType[file.ContentType]}";

        await using (var stream = file.OpenReadStream())
            await blobs.UploadAsync(blobName, stream, file.ContentType);

        var image = new PostImage
        {
            Id = imageId,
            PostId = postId,
            BlobName = blobName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
        };
        try
        {
            db.PostImages.Add(image);
            await db.SaveChangesAsync();
        }
        catch
        {
            await TryDeleteBlob(blobName);
            throw;
        }

        return CreatedAtAction(nameof(DownloadImage), new { imageId = image.Id }, PostsController.ToImage(image));
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
        if (!await db.Posts.AnyAsync(p => p.Id == postId)) return PostNotFound();

        var images = await db.PostImages.AsNoTracking()
            .Where(i => i.PostId == postId)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync();

        return Ok(images.Select(PostsController.ToImage).ToList());
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
        var image = await db.PostImages.AsNoTracking().SingleOrDefaultAsync(i => i.Id == imageId);
        if (image is null) return ImageNotFound();
        try
        {
            var (content, contentType, _) = await blobs.OpenReadAsync(image.BlobName);
            return File(content, contentType);
        }
        catch (FileNotFoundException)
        {
            return ImageNotFound();
        }
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
        var image = await db.PostImages.AsNoTracking().SingleOrDefaultAsync(i => i.Id == imageId);
        if (image is null) return ImageNotFound();
        try
        {
            var (content, contentType, _) = await blobs.OpenReadAsync(image.BlobName);
            return File(content, contentType, $"{imageId:N}{Path.GetExtension(image.BlobName)}");
        }
        catch (FileNotFoundException)
        {
            return ImageNotFound();
        }
    }

    /// <summary>Elimina una imagen de tu publicación (borra el metadato y el blob en Azure). Solo el autor.</summary>
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
        var image = await db.PostImages.Include(i => i.Post).SingleOrDefaultAsync(i => i.Id == imageId);
        if (image is null) return ImageNotFound();
        if (image.Post.AuthorId != currentUser.UserId) return NotOwner();

        db.PostImages.Remove(image);
        await db.SaveChangesAsync();
        await TryDeleteBlob(image.BlobName);
        return NoContent();
    }

    private async Task TryDeleteBlob(string blobName)
    {
        try { await blobs.DeleteAsync(blobName); }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"No se pudo eliminar el blob '{blobName}'.", ex);
        }
    }

    private ProblemDetails SimpleProblem(int status, string title, string detail, string code) => new()
    {
        Status = status, Title = title, Detail = detail,
        Extensions = { ["code"] = code, ["traceId"] = HttpContext.TraceIdentifier },
    };

    private ActionResult PostNotFound() => NotFound(
        SimpleProblem(404, "Publicación no encontrada", "No existe una publicación con ese id.", Middleware.ErrorCodes.NotFound));

    private ActionResult ImageNotFound() => NotFound(
        SimpleProblem(404, "Imagen no encontrada", "No existe una imagen con ese id.", Middleware.ErrorCodes.NotFound));

    private ActionResult NotOwner() => StatusCode(403,
        SimpleProblem(403, "No eres el autor", "Solo el autor puede modificar las imágenes de esta publicación.", Middleware.ErrorCodes.ForbiddenNotOwner));
}
