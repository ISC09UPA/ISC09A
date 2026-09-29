using System.ComponentModel.DataAnnotations;
using ReportU.Models;

namespace ReportU.Dtos;

/// <summary>Registro de un estudiante nuevo.</summary>
public class RegisterRequest
{
    /// <example>ana.perez@universidad.edu.mx</example>
    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    /// <example>MiPassword123</example>
    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    /// <example>ana_p</example>
    [Required, MinLength(3), MaxLength(30), RegularExpression(@"^[a-zA-Z0-9_.]+$")]
    public string Username { get; set; } = string.Empty;

    /// <example>Ana Pérez López</example>
    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    /// <example>Ingeniería en Sistemas</example>
    [Required, MaxLength(120)]
    public string Career { get; set; } = string.Empty;

    /// <example>202312345</example>
    [Required, MaxLength(30)]
    public string EnrollmentNumber { get; set; } = string.Empty;
}

/// <summary>Inicio de sesión. Acepta email o username como identificador.</summary>
public class LoginRequest
{
    /// <example>ana.perez@universidad.edu.mx</example>
    [Required, MaxLength(320)]
    public string Identifier { get; set; } = string.Empty;

    /// <example>MiPassword123</example>
    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Respuesta de autenticación: token JWT + perfil básico.</summary>
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;

    /// <example>Bearer</example>
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAt { get; set; }
    public UserResponse User { get; set; } = null!;
}
