using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportU.Data;
using ReportU.Dtos;
using ReportU.Models;
using ReportU.Services;

namespace ReportU.Controllers;

/// <summary>Feed y CRUD de publicaciones. Solo el autor puede editar o eliminar su publicación.</summary>
[ApiController]
[Route("api/posts")]
[Authorize]
[Produces("application/json")]
public class PostsController(
    ReportUDbContext db,
    ICurrentUserService currentUser,
    IBlobStorageService blobs,
    ILogger<PostsController> logger) : ControllerBase
{
    /// <summary>Feed principal con las publicaciones de todos los estudiantes.</summary>
    /// <param name="sort">`recent` = Recientes (createdAt DESC) · `popular` = Populares (supportCount DESC).</param>
    /// <param name="page">Página base 1.</param>
    /// <param name="pageSize">Tamaño de página, 1–50.</param>
    /// <response code="200">Página del feed.</response>
    /// <response code="400">Parámetros de paginación inválidos.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PostSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<PostSummaryResponse>>> GetFeed(
        [FromQuery] string sort = "recent",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize is < 1 or > 50)
            return BadRequest(new ProblemDetails
            {
                Status = 400, Title = "Paginación inválida",
                Detail = "page debe ser >= 1 y pageSize estar entre 1 y 50.",
                Extensions = { ["code"] = Middleware.ErrorCodes.ValidationError, ["traceId"] = HttpContext.TraceIdentifier },
            });

        var query = db.Posts.AsNoTracking().Include(p => p.Author).AsQueryable();
        query = sort.ToLowerInvariant() == "popular"
            ? query.OrderByDescending(p => p.SupportCount).ThenByDescending(p => p.CreatedAt)
            : query.OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync();
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Images)
            .ToListAsync();

        var me = currentUser.UserId!.Value;
        var ids = posts.Select(p => p.Id).ToList();
        var supported = await db.PostSupports.Where(s => s.UserId == me && ids.Contains(s.PostId))
            .Select(s => s.PostId).ToListAsync();
        var bookmarked = await db.Bookmarks.Where(x => x.UserId == me && ids.Contains(x.PostId))
            .Select(x => x.PostId).ToListAsync();

        return Ok(new PagedResult<PostSummaryResponse>
        {
            Items = posts.Select(p => ToSummary(p, supported.Contains(p.Id), bookmarked.Contains(p.Id))).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
        });
    }

    /// <summary>Obtiene el detalle de una publicación con sus imágenes y contadores.</summary>
    /// <param name="id">Id de la publicación.</param>
    /// <response code="200">Detalle de la publicación.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">La publicación no existe (`not_found`).</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PostDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostDetailResponse>> GetById([FromRoute] Guid id)
    {
        var post = await db.Posts.AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Images)
            .SingleOrDefaultAsync(p => p.Id == id);
        if (post is null) return PostNotFound();

        var me = currentUser.UserId!.Value;
        var supported = await db.PostSupports.AnyAsync(s => s.PostId == id && s.UserId == me);
        var bookmarked = await db.Bookmarks.AnyAsync(x => x.PostId == id && x.UserId == me);

        var detail = ToSummary(post, supported, bookmarked);
        return Ok(new PostDetailResponse
        {
            Id = detail.Id, Title = detail.Title, Description = post.Description,
            Type = detail.Type, Category = detail.Category, Location = detail.Location,
            Author = detail.Author, SupportCount = detail.SupportCount, CommentCount = detail.CommentCount,
            ImageCount = detail.ImageCount, CoverImage = detail.CoverImage,
            SupportedByMe = supported, BookmarkedByMe = bookmarked,
            CreatedAt = detail.CreatedAt, UpdatedAt = detail.UpdatedAt,
            Images = post.Images.OrderBy(i => i.CreatedAt).Select(ToImage).ToList(),
        });
    }

    /// <summary>Crea una publicación propia. Las imágenes se agregan después con POST /api/posts/{id}/images.</summary>
    /// <param name="request">Título, descripción, tipo, categoría, ubicación e identidad.</param>
    /// <response code="201">Publicación creada.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PostDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PostDetailResponse>> Create([FromBody] CreatePostRequest request)
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = currentUser.UserId!.Value,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Type = request.Type,
            Category = request.Category,
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            IdentityMode = request.IdentityMode,
        };
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var created = await GetById(post.Id);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, ((OkObjectResult)created.Result!).Value);
    }

    /// <summary>Edita parcialmente una publicación propia.</summary>
    /// <param name="id">Id de la publicación.</param>
    /// <param name="request">Solo se aplican los campos presentes.</param>
    /// <response code="200">Publicación actualizada.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="403">No eres el autor (`forbidden_not_owner`).</response>
    /// <response code="404">La publicación no existe.</response>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(PostDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostDetailResponse>> Update([FromRoute] Guid id, [FromBody] UpdatePostRequest request)
    {
        var post = await db.Posts.SingleOrDefaultAsync(p => p.Id == id);
        if (post is null) return PostNotFound();
        if (post.AuthorId != currentUser.UserId) return NotOwner();

        if (request.Title is not null) post.Title = request.Title.Trim();
        if (request.Description is not null) post.Description = request.Description.Trim();
        if (request.Type.HasValue) post.Type = request.Type.Value;
        if (request.Category.HasValue) post.Category = request.Category.Value;
        if (request.Location is not null)
            post.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        if (request.IdentityMode.HasValue) post.IdentityMode = request.IdentityMode.Value;

        await db.SaveChangesAsync();
        return await GetById(id);
    }

    /// <summary>Elimina una publicación propia junto con sus imágenes (también del Blob Storage), comentarios, apoyos y guardados.</summary>
    /// <param name="id">Id de la publicación.</param>
    /// <response code="204">Publicación eliminada.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="403">No eres el autor (`forbidden_not_owner`).</response>
    /// <response code="404">La publicación no existe.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var post = await db.Posts.Include(p => p.Images).SingleOrDefaultAsync(p => p.Id == id);
        if (post is null) return PostNotFound();
        if (post.AuthorId != currentUser.UserId) return NotOwner();

        var blobNames = post.Images.Select(i => i.BlobName).ToList();
        db.Posts.Remove(post);
        await db.SaveChangesAsync();

        foreach (var blob in blobNames)
        {
            try { await blobs.DeleteAsync(blob); }
            catch (Exception ex) { logger.LogWarning(ex, "No se pudo borrar el blob {Blob} del post {PostId}.", blob, id); }
        }
        return NoContent();
    }

    private ActionResult PostNotFound() => NotFound(new ProblemDetails
    {
        Status = 404, Title = "Publicación no encontrada",
        Detail = "No existe una publicación con ese id.",
        Extensions = { ["code"] = Middleware.ErrorCodes.NotFound, ["traceId"] = HttpContext.TraceIdentifier },
    });

    private ActionResult NotOwner() => StatusCode(403, new ProblemDetails
    {
        Status = 403, Title = "No eres el autor",
        Detail = "Solo el autor puede editar o eliminar esta publicación.",
        Extensions = { ["code"] = Middleware.ErrorCodes.ForbiddenNotOwner, ["traceId"] = HttpContext.TraceIdentifier },
    });

    internal static PostSummaryResponse ToSummary(Post p, bool supportedByMe, bool bookmarkedByMe)
    {
        var ordered = p.Images.OrderBy(i => i.CreatedAt).ToList();
        return new PostSummaryResponse
        {
            Id = p.Id, Title = p.Title, Type = p.Type, Category = p.Category, Location = p.Location,
            Author = AuthorDisplay.Resolve(p.Author, p.IdentityMode),
            SupportCount = p.SupportCount, CommentCount = p.CommentCount,
            ImageCount = ordered.Count,
            CoverImage = ordered.Count == 0 ? null : ToImage(ordered[0]),
            SupportedByMe = supportedByMe, BookmarkedByMe = bookmarkedByMe,
            CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt,
        };
    }

    internal static PostImageResponse ToImage(PostImage i) => new()
    {
        Id = i.Id,
        Url = $"/api/media/{i.Id}",
        DownloadUrl = $"/api/media/{i.Id}/download",
        ContentType = i.ContentType,
        SizeBytes = i.SizeBytes,
        CreatedAt = i.CreatedAt,
    };
}
