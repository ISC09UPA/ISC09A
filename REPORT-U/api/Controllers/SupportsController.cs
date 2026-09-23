using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportU.Data;
using ReportU.Dtos;
using ReportU.Models;
using ReportU.Services;

namespace ReportU.Controllers;

/// <summary>Apoyos: 1 usuario + 1 publicación = máximo 1 apoyo. Apoyar y retirar apoyo.</summary>
[ApiController]
[Route("api/posts/{postId:guid}/support")]
[Authorize]
[Produces("application/json")]
public class SupportsController(ReportUDbContext db, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>Apoya una publicación.</summary>
    /// <param name="postId">Id de la publicación.</param>
    /// <response code="200">Apoyo registrado (devuelve el contador actualizado).</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">La publicación no existe.</response>
    /// <response code="409">Ya habías apoyado esta publicación (`already_supported`).</response>
    [HttpPut]
    [ProducesResponseType(typeof(SupportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SupportResponse>> Support([FromRoute] Guid postId)
    {
        var post = await db.Posts.SingleOrDefaultAsync(p => p.Id == postId);
        if (post is null) return PostNotFound();

        var me = currentUser.UserId!.Value;
        if (await db.PostSupports.AnyAsync(s => s.PostId == postId && s.UserId == me))
            return Conflict(new ProblemDetails
            {
                Status = 409, Title = "Ya apoyaste esta publicación",
                Detail = "Un usuario solo puede apoyar una vez cada publicación.",
                Extensions = { ["code"] = Middleware.ErrorCodes.AlreadySupported, ["traceId"] = HttpContext.TraceIdentifier },
            });

        db.PostSupports.Add(new PostSupport { PostId = postId, UserId = me });
        post.SupportCount++;
        await db.SaveChangesAsync();

        return Ok(new SupportResponse { SupportCount = post.SupportCount, SupportedByMe = true });
    }

    /// <summary>Retira tu apoyo de una publicación.</summary>
    /// <param name="postId">Id de la publicación.</param>
    /// <response code="200">Apoyo retirado (devuelve el contador actualizado).</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">La publicación no existe o no la habías apoyado (`not_supported`).</response>
    [HttpDelete]
    [ProducesResponseType(typeof(SupportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupportResponse>> Unsupport([FromRoute] Guid postId)
    {
        var post = await db.Posts.SingleOrDefaultAsync(p => p.Id == postId);
        if (post is null) return PostNotFound();

        var me = currentUser.UserId!.Value;
        var support = await db.PostSupports.SingleOrDefaultAsync(s => s.PostId == postId && s.UserId == me);
        if (support is null)
            return NotFound(new ProblemDetails
            {
                Status = 404, Title = "No habías apoyado esta publicación",
                Detail = "No hay apoyo tuyo que retirar.",
                Extensions = { ["code"] = Middleware.ErrorCodes.NotSupported, ["traceId"] = HttpContext.TraceIdentifier },
            });

        db.PostSupports.Remove(support);
        post.SupportCount = Math.Max(0, post.SupportCount - 1);
        await db.SaveChangesAsync();

        return Ok(new SupportResponse { SupportCount = post.SupportCount, SupportedByMe = false });
    }

    private ActionResult PostNotFound() => NotFound(new ProblemDetails
    {
        Status = 404, Title = "Publicación no encontrada",
        Detail = "No existe una publicación con ese id.",
        Extensions = { ["code"] = Middleware.ErrorCodes.NotFound, ["traceId"] = HttpContext.TraceIdentifier },
    });
}
