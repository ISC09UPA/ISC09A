using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportU.Data;
using ReportU.Dtos;
using ReportU.Models;
using ReportU.Services;

namespace ReportU.Controllers;

/// <summary>Guardados del estudiante y "Mis publicaciones".</summary>
[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public class MeController(ReportUDbContext db, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>Lista las publicaciones creadas por el estudiante autenticado (más recientes primero).</summary>
    /// <param name="page">Página base 1.</param>
    /// <param name="pageSize">Tamaño de página, 1–50.</param>
    /// <response code="200">Página de publicaciones propias. Desde aquí la app permite ver, editar y eliminar.</response>
    /// <response code="400">Paginación inválida.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    [HttpGet("me/posts")]
    [ProducesResponseType(typeof(PagedResult<PostSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<PostSummaryResponse>>> GetMyPosts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize is < 1 or > 50)
            return BadRequest(new ProblemDetails
            {
                Status = 400, Title = "Paginación inválida",
                Detail = "page debe ser >= 1 y pageSize estar entre 1 y 50.",
                Extensions = { ["code"] = Middleware.ErrorCodes.ValidationError, ["traceId"] = HttpContext.TraceIdentifier },
            });

        var me = currentUser.UserId!.Value;
        var query = db.Posts.AsNoTracking()
            .Include(p => p.Author)
            .Where(p => p.AuthorId == me)
            .OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync();
        var posts = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Include(p => p.Images).ToListAsync();

        var ids = posts.Select(p => p.Id).ToList();
        var supported = await db.PostSupports.Where(s => s.UserId == me && ids.Contains(s.PostId))
            .Select(s => s.PostId).ToListAsync();
        var bookmarked = await db.Bookmarks.Where(x => x.UserId == me && ids.Contains(x.PostId))
            .Select(x => x.PostId).ToListAsync();

        return Ok(new PagedResult<PostSummaryResponse>
        {
            Items = posts.Select(p => PostsController.ToSummary(p, supported.Contains(p.Id), bookmarked.Contains(p.Id))).ToList(),
            Page = page, PageSize = pageSize, TotalCount = total,
        });
    }

    /// <summary>Lista las publicaciones guardadas por el estudiante (pantalla "Guardados").</summary>
    /// <param name="page">Página base 1.</param>
    /// <param name="pageSize">Tamaño de página, 1–50.</param>
    /// <response code="200">Página de publicaciones guardadas.</response>
    /// <response code="400">Paginación inválida.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    [HttpGet("me/bookmarks")]
    [ProducesResponseType(typeof(PagedResult<PostSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<PostSummaryResponse>>> GetMyBookmarks(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize is < 1 or > 50)
            return BadRequest(new ProblemDetails
            {
                Status = 400, Title = "Paginación inválida",
                Detail = "page debe ser >= 1 y pageSize estar entre 1 y 50.",
                Extensions = { ["code"] = Middleware.ErrorCodes.ValidationError, ["traceId"] = HttpContext.TraceIdentifier },
            });

        var me = currentUser.UserId!.Value;
        var bookmarkedQuery = db.Bookmarks.AsNoTracking().Where(x => x.UserId == me);

        var total = await bookmarkedQuery.CountAsync();
        var ids = await bookmarkedQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => x.PostId)
            .ToListAsync();

        var posts = await db.Posts.AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
        // Mantiene el orden de guardado (más reciente primero).
        var ordered = ids.Select(id => posts.Single(p => p.Id == id)).ToList();

        var supported = await db.PostSupports.Where(s => s.UserId == me && ids.Contains(s.PostId))
            .Select(s => s.PostId).ToListAsync();

        return Ok(new PagedResult<PostSummaryResponse>
        {
            Items = ordered.Select(p => PostsController.ToSummary(p, supported.Contains(p.Id), true)).ToList(),
            Page = page, PageSize = pageSize, TotalCount = total,
        });
    }

    /// <summary>Guarda una publicación en tu lista de guardados.</summary>
    /// <param name="postId">Id de la publicación.</param>
    /// <response code="200">Publicación guardada.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">La publicación no existe.</response>
    /// <response code="409">Ya la tenías guardada (`already_bookmarked`).</response>
    [HttpPut("posts/{postId:guid}/bookmark")]
    [ProducesResponseType(typeof(BookmarkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookmarkResponse>> Bookmark([FromRoute] Guid postId)
    {
        if (!await db.Posts.AnyAsync(p => p.Id == postId))
            return NotFound(new ProblemDetails
            {
                Status = 404, Title = "Publicación no encontrada",
                Detail = "No existe una publicación con ese id.",
                Extensions = { ["code"] = Middleware.ErrorCodes.NotFound, ["traceId"] = HttpContext.TraceIdentifier },
            });

        var me = currentUser.UserId!.Value;
        if (await db.Bookmarks.AnyAsync(x => x.PostId == postId && x.UserId == me))
            return Conflict(new ProblemDetails
            {
                Status = 409, Title = "Ya guardaste esta publicación",
                Detail = "La publicación ya está en tus guardados.",
                Extensions = { ["code"] = Middleware.ErrorCodes.AlreadyBookmarked, ["traceId"] = HttpContext.TraceIdentifier },
            });

        db.Bookmarks.Add(new Bookmark { PostId = postId, UserId = me });
        await db.SaveChangesAsync();
        return Ok(new BookmarkResponse { BookmarkedByMe = true });
    }

    /// <summary>Quita una publicación de tus guardados.</summary>
    /// <param name="postId">Id de la publicación.</param>
    /// <response code="200">Guardado eliminado.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">La publicación no existe o no la tenías guardada (`not_bookmarked`).</response>
    [HttpDelete("posts/{postId:guid}/bookmark")]
    [ProducesResponseType(typeof(BookmarkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookmarkResponse>> Unbookmark([FromRoute] Guid postId)
    {
        var me = currentUser.UserId!.Value;
        var bookmark = await db.Bookmarks.SingleOrDefaultAsync(x => x.PostId == postId && x.UserId == me);
        if (bookmark is null)
            return NotFound(new ProblemDetails
            {
                Status = 404, Title = "No tenías guardada esta publicación",
                Detail = "No hay guardado tuyo que eliminar.",
                Extensions = { ["code"] = Middleware.ErrorCodes.NotBookmarked, ["traceId"] = HttpContext.TraceIdentifier },
            });

        db.Bookmarks.Remove(bookmark);
        await db.SaveChangesAsync();
        return Ok(new BookmarkResponse { BookmarkedByMe = false });
    }
}
