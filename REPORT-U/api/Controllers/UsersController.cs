using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportU.Data;
using ReportU.Dtos;
using ReportU.Services;

namespace ReportU.Controllers;

/// <summary>Perfil propio del estudiante autenticado.</summary>
[ApiController]
[Route("api/users")]
[Authorize]
[Produces("application/json")]
public class UsersController(ReportUDbContext db, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>Obtiene el perfil propio (incluye email; el email nunca es público en otros endpoints).</summary>
    /// <response code="200">Perfil del estudiante autenticado.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">El usuario del token ya no existe.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == currentUser.UserId);
        return user is null ? NotFoundProblem() : Ok(AuthController.ToUserResponse(user));
    }

    /// <summary>Actualiza parcialmente el perfil propio (nombre, carrera, preferencia de identidad, matrícula visible/oculta).</summary>
    /// <param name="request">Solo se aplican los campos presentes. Email, username y matrícula no son editables en el MVP.</param>
    /// <response code="200">Perfil actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">Falta el token o es inválido.</response>
    /// <response code="404">El usuario del token ya no existe.</response>
    [HttpPatch("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateMe([FromBody] UpdateMeRequest request)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == currentUser.UserId);
        if (user is null) return NotFoundProblem();

        if (request.FullName is not null) user.FullName = request.FullName.Trim();
        if (request.Career is not null) user.Career = request.Career.Trim();
        if (request.DisplayNamePreference.HasValue) user.DisplayNamePreference = request.DisplayNamePreference.Value;
        if (request.ShowEnrollmentNumber.HasValue) user.ShowEnrollmentNumber = request.ShowEnrollmentNumber.Value;

        await db.SaveChangesAsync();
        return Ok(AuthController.ToUserResponse(user));
    }

    private ActionResult NotFoundProblem() => NotFound(new ProblemDetails
    {
        Status = 404, Title = "Usuario no encontrado",
        Detail = "El usuario del token ya no existe.",
        Extensions = { ["code"] = Middleware.ErrorCodes.NotFound, ["traceId"] = HttpContext.TraceIdentifier },
    });
}
