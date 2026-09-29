using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportU.Data;
using ReportU.Dtos;
using ReportU.Models;
using ReportU.Services;

namespace ReportU.Controllers;

/// <summary>Registro, inicio y cierre de sesión (el "cierre" es borrar el token en el cliente: JWT sin estado).</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(
    ReportUDbContext db,
    IPasswordService passwords,
    IJwtTokenService tokens) : ControllerBase
{
    /// <summary>Registra un estudiante nuevo y devuelve su primer token de acceso.</summary>
    /// <param name="request">Datos básicos del estudiante.</param>
    /// <response code="201">Estudiante creado. Usar el token como `Authorization: Bearer {token}`.</response>
    /// <response code="400">Datos inválidos (ver `errors` del ProblemDetails).</response>
    /// <response code="409">El email (`email_taken`) o username (`username_taken`) ya está registrado.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var username = request.Username.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new ProblemDetails
            {
                Status = 409, Title = "Email ya registrado",
                Detail = "Ya existe un estudiante con ese email.",
                Extensions = { ["code"] = Middleware.ErrorCodes.EmailTaken, ["traceId"] = HttpContext.TraceIdentifier },
            });
        if (await db.Users.AnyAsync(u => u.Username == username))
            return Conflict(new ProblemDetails
            {
                Status = 409, Title = "Username ya registrado",
                Detail = "Ya existe un estudiante con ese username.",
                Extensions = { ["code"] = Middleware.ErrorCodes.UsernameTaken, ["traceId"] = HttpContext.TraceIdentifier },
            });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwords.Hash(request.Password),
            Username = username,
            FullName = request.FullName.Trim(),
            Career = request.Career.Trim(),
            EnrollmentNumber = request.EnrollmentNumber.Trim(),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var (token, expiresAt) = tokens.CreateToken(user.Id, user.Email, user.Username);
        return CreatedAtAction(nameof(Register), new AuthResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            User = ToUserResponse(user),
        });
    }

    /// <summary>Inicia sesión con email o username y devuelve un token de acceso.</summary>
    /// <param name="request">Identificador (email o username) + contraseña.</param>
    /// <response code="200">Autenticación correcta.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">Credenciales incorrectas (`invalid_credentials`).</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var identifier = request.Identifier.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == identifier || u.Username == identifier);

        if (user is null || !passwords.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new ProblemDetails
            {
                Status = 401, Title = "Credenciales inválidas",
                Detail = "El identificador o la contraseña son incorrectos.",
                Extensions = { ["code"] = Middleware.ErrorCodes.InvalidCredentials, ["traceId"] = HttpContext.TraceIdentifier },
            });

        var (token, expiresAt) = tokens.CreateToken(user.Id, user.Email, user.Username);
        return Ok(new AuthResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            User = ToUserResponse(user),
        });
    }

    internal static UserResponse ToUserResponse(User u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        Username = u.Username,
        FullName = u.FullName,
        Career = u.Career,
        EnrollmentNumber = u.EnrollmentNumber,
        DisplayNamePreference = u.DisplayNamePreference,
        ShowEnrollmentNumber = u.ShowEnrollmentNumber,
        CreatedAt = u.CreatedAt,
    };
}
