using Microsoft.EntityFrameworkCore;
using ReportU.Data;
using ReportU.Middleware;
using ReportU.Models;

namespace ReportU.Services;

/// <summary>
/// Error de dominio del manejo de imágenes con su mapeo HTTP.
/// El <see cref="ExceptionHandlingMiddleware"/> lo convierte en ProblemDetails
/// con el <c>status</c> y <c>code</c> correspondientes, así los controladores
/// no necesitan try/catch ni conocer Azure.
/// </summary>
public class PostImageException(int status, string code, string title, string detail)
    : Exception(detail)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
    public string Title { get; } = title;

    public static PostImageException PostNotFound() => new PostImageException(
        404, ErrorCodes.NotFound, "Publicación no encontrada",
        "No existe una publicación con ese id.");

    public static PostImageException ImageNotFound() => new PostImageException(
        404, ErrorCodes.NotFound, "Imagen no encontrada",
        "No existe una imagen con ese id.");

    public static PostImageException NotOwner() => new PostImageException(
        403, ErrorCodes.ForbiddenNotOwner, "No eres el autor",
        "Solo el autor puede modificar las imágenes de esta publicación.");

    public static PostImageException EmptyFile() => new PostImageException(
        400, ErrorCodes.ValidationError, "Archivo vacío",
        "Debes enviar una imagen como multipart/form-data.");

    public static PostImageException UnsupportedType() => new PostImageException(
        415, ErrorCodes.UnsupportedMediaType, "Tipo no permitido",
        "Solo se permiten imágenes jpg, png, webp o gif.");

    public static PostImageException TooLarge() => new PostImageException(
        413, ErrorCodes.PayloadTooLarge, "Imagen muy grande",
        "Cada imagen debe pesar máximo 5 MB.");

    public static PostImageException LimitReached() => new PostImageException(
        409, ErrorCodes.ImageLimitReached, "Límite de imágenes",
        "Una publicación puede tener máximo 4 imágenes.");

    public static PostImageException BlobDeleteFailed(string blobName, Exception inner) => new PostImageException(
        502, ErrorCodes.BlobError, "Error en almacenamiento",
        $"La imagen se eliminó del catálogo pero no se pudo borrar el blob '{blobName}'. Reintenta el borrado.")
        .WithInner(inner);

    private PostImageException WithInner(Exception inner)
    {
        Data["Cause"] = inner.ToString();
        return this;
    }
}

/// <summary>
/// Caso de uso de imágenes de publicaciones (<see cref="PostImage"/>).
/// Toda la orquestación entre EF Core y el <see cref="IBlobStorageService"/>
/// (Azure o local) vive aquí: los controladores solo llaman al servicio y mapean a DTOs.
/// </summary>
public interface IPostImageService
{
    /// <summary>Sube el binario al storage y crea el metadato asociado al post. Solo el autor.</summary>
    /// <exception cref="PostImageException">Post inexistente, no autor, archivo inválido, límite alcanzado.</exception>
    Task<PostImage> UploadAsync(Guid postId, Guid requesterId, Stream content, string contentType, long length, CancellationToken ct = default);

    /// <summary>Metadatos ordenados por fecha de subida.</summary>
    /// <exception cref="PostImageException">Post inexistente.</exception>
    Task<IReadOnlyList<PostImage>> ListAsync(Guid postId, CancellationToken ct = default);

    /// <summary>Abre el binario desde el storage.</summary>
    /// <exception cref="PostImageException">Imagen inexistente (ni en BD ni en el blob).</exception>
    Task<(Stream Content, string ContentType, long Length)> OpenReadAsync(Guid imageId, CancellationToken ct = default);

    /// <summary>Borra metadato + blob. Solo el autor del post.</summary>
    /// <exception cref="PostImageException">Imagen inexistente, no autor o fallo del blob (502).</exception>
    Task DeleteAsync(Guid imageId, Guid requesterId, CancellationToken ct = default);

    /// <summary>
    /// Limpieza best-effort de blobs huérfanos tras borrar un post
    /// (la BD ya eliminó los metadatos por cascada). Nunca lanza.
    /// </summary>
    Task PurgePostBlobsAsync(Guid postId, IEnumerable<string> blobNames, CancellationToken ct = default);
}

public class PostImageService(
    ReportUDbContext db,
    IBlobStorageService blobs,
    ILogger<PostImageService> logger) : IPostImageService
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

    public async Task<PostImage> UploadAsync(Guid postId, Guid requesterId, Stream content, string contentType, long length, CancellationToken ct = default)
    {
        var post = await db.Posts.Include(p => p.Images).SingleOrDefaultAsync(p => p.Id == postId, ct);
        if (post is null) throw PostImageException.PostNotFound();
        if (post.AuthorId != requesterId) throw PostImageException.NotOwner();

        if (length <= 0) throw PostImageException.EmptyFile();
        if (!AllowedContentTypes.Contains(contentType)) throw PostImageException.UnsupportedType();
        if (length > MaxImageBytes) throw PostImageException.TooLarge();
        if (post.Images.Count >= MaxImagesPerPost) throw PostImageException.LimitReached();

        var imageId = Guid.NewGuid();
        var blobName = $"{postId:N}/{imageId:N}{ExtensionByContentType[contentType]}";

        await blobs.UploadAsync(blobName, content, contentType, ct);

        var image = new PostImage
        {
            Id = imageId,
            PostId = postId,
            BlobName = blobName,
            ContentType = contentType,
            SizeBytes = length,
        };
        try
        {
            db.PostImages.Add(image);
            await db.SaveChangesAsync(ct);
            return image;
        }
        catch
        {
            // Compensación: el blob ya subió pero el metadato no se guardó.
            try { await blobs.DeleteAsync(blobName, ct); }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Blob huérfano {Blob}: subió al storage pero falló el guardado en BD.", blobName);
            }
            throw;
        }
    }

    public async Task<IReadOnlyList<PostImage>> ListAsync(Guid postId, CancellationToken ct = default)
    {
        if (!await db.Posts.AnyAsync(p => p.Id == postId, ct))
            throw PostImageException.PostNotFound();

        return await db.PostImages.AsNoTracking()
            .Where(i => i.PostId == postId)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<(Stream Content, string ContentType, long Length)> OpenReadAsync(Guid imageId, CancellationToken ct = default)
    {
        var image = await db.PostImages.AsNoTracking().SingleOrDefaultAsync(i => i.Id == imageId, ct);
        if (image is null) throw PostImageException.ImageNotFound();
        try
        {
            return await blobs.OpenReadAsync(image.BlobName, ct);
        }
        catch (FileNotFoundException ex)
        {
            throw PostImageException.ImageNotFound().WithLoggedWarning(logger, ex, image.BlobName);
        }
    }

    public async Task DeleteAsync(Guid imageId, Guid requesterId, CancellationToken ct = default)
    {
        var image = await db.PostImages.Include(i => i.Post).SingleOrDefaultAsync(i => i.Id == imageId, ct);
        if (image is null) throw PostImageException.ImageNotFound();
        if (image.Post.AuthorId != requesterId) throw PostImageException.NotOwner();

        var blobName = image.BlobName;
        db.PostImages.Remove(image);
        await db.SaveChangesAsync(ct);

        try
        {
            await blobs.DeleteAsync(blobName, ct);
        }
        catch (Exception ex)
        {
            throw PostImageException.BlobDeleteFailed(blobName, ex);
        }
    }

    public async Task PurgePostBlobsAsync(Guid postId, IEnumerable<string> blobNames, CancellationToken ct = default)
    {
        foreach (var blob in blobNames)
        {
            try
            {
                await blobs.DeleteAsync(blob, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo borrar el blob {Blob} del post {PostId}.", blob, postId);
            }
        }
    }
}

internal static class PostImageExceptionLogging
{
    public static PostImageException WithLoggedWarning(
        this PostImageException ex, ILogger logger, Exception cause, string blobName)
    {
        logger.LogWarning(cause, "Blob {Blob} referenciado en BD pero ausente en el storage.", blobName);
        return ex;
    }
}
