using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportU.Data;
using ReportU.Dtos;
using ReportU.Models;
using ReportU.Services;

namespace ReportU.Controllers;

/// <summary>
/// Apoyos: 1 usuario + 1 publicación = máximo 1 apoyo. La unicidad la garantiza
/// la PK compuesta (PostId, UserId) de post_supports, también si dos requests
/// del mismo usuario corren en paralelo (el perdedor recibe 409, no un 500).
/// El contador se actualiza con SQL atómico para no perder incrementos cuando
/// dos usuarios apoyan a la vez.
/// </summary>
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
        if (!await db.Posts.AnyAsync(p => p.Id == postId)) return PostNotFound();

        var me = currentUser.UserId!.Value;
        if (await db.PostSupports.AnyAsync(s => s.PostId == postId && s.UserId == me))
            return AlreadySupported();

        // Insertar primero: la PK compuesta (PostId, UserId) aplica la regla
        // "máximo 1 apoyo por usuario" incluso contra requests paralelos.
        db.PostSupports.Add(new PostSupport { PostId = postId, UserId = me });
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Perdió la carrera contra un PUT paralelo del mismo usuario: la
            // restricción única rechazó el insert. Respuesta 409 (nunca 500).
            var failed = db.ChangeTracker.Entries<PostSupport>()
                .FirstOrDefault(e => e.Entity.PostId == postId && e.Entity.UserId == me);
            if (failed is not null) failed.State = EntityState.Detached;
            return AlreadySupported();
        }

        // Incremento atómico en SQL (no lectura-modificación-escritura en memoria):
        // dos usuarios que apoyan a la vez no pierden su incremento.
        await db.Posts.Where(p => p.Id == postId)
            .ExecuteUpdateAsync(u => u.SetProperty(p => p.SupportCount, p => p.SupportCount + 1));

        return Ok(new SupportResponse { SupportCount = await CountAsync(postId), SupportedByMe = true });
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
        if (!await db.Posts.AnyAsync(p => p.Id == postId)) return PostNotFound();

        var me = currentUser.UserId!.Value;
        if (!await db.PostSupports.AnyAsync(s => s.PostId == postId && s.UserId == me))
            return NotSupported();

        // Borrado por claves: si un request paralelo ya lo retiró, borra 0 filas.
        var removed = await db.PostSupports
            .Where(s => s.PostId == postId && s.UserId == me)
            .ExecuteDeleteAsync();
        if (removed == 0) return NotSupported();

        // Decremento atómico en SQL y nunca negativo.
        await db.Posts.Where(p => p.Id == postId && p.SupportCount > 0)
            .ExecuteUpdateAsync(u => u.SetProperty(p => p.SupportCount, p => p.SupportCount - 1));

        return Ok(new SupportResponse { SupportCount = await CountAsync(postId), SupportedByMe = false });
    }

    // Fuente de verdad para la respuesta: contar la tabla de apoyos evita que el
    // contador desnormalizado quede desincronizado tras carreras o reintentos.
    private Task<int> CountAsync(Guid postId) => db.PostSupports.CountAsync(s => s.PostId == postId);

    private ActionResult AlreadySupported() => Conflict(new ProblemDetails
    {
        Status = 409, Title = "Ya apoyaste esta publicación",
        Detail = "Un usuario solo puede apoyar una vez cada publicación.",
        Extensions = { ["code"] = Middleware.ErrorCodes.AlreadySupported, ["traceId"] = HttpContext.TraceIdentifier },
    });

    private ActionResult NotSupported() => NotFound(new ProblemDetails
    {
        Status = 404, Title = "No habías apoyado esta publicación",
        Detail = "No hay apoyo tuyo que retirar.",
        Extensions = { ["code"] = Middleware.ErrorCodes.NotSupported, ["traceId"] = HttpContext.TraceIdentifier },
    });

    private ActionResult PostNotFound() => NotFound(new ProblemDetails
    {
        Status = 404, Title = "Publicación no encontrada",
        Detail = "No existe una publicación con ese id.",
        Extensions = { ["code"] = Middleware.ErrorCodes.NotFound, ["traceId"] = HttpContext.TraceIdentifier },
    });
}
