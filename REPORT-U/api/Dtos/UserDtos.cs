using System.ComponentModel.DataAnnotations;
using ReportU.Models;

namespace ReportU.Dtos;

/// <summary>
/// Perfil propio del estudiante autenticado. Es el único lugar donde el email es visible:
/// el email nunca se expone en autores de publicaciones ni comentarios.
/// </summary>
public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Career { get; set; } = string.Empty;
    public string EnrollmentNumber { get; set; } = string.Empty;
    public DisplayNamePreference DisplayNamePreference { get; set; }
    public bool ShowEnrollmentNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Actualización parcial del perfil propio. Todo es opcional.</summary>
public class UpdateMeRequest
{
    [MaxLength(120)]
    public string? FullName { get; set; }

    [MaxLength(120)]
    public string? Career { get; set; }

    public DisplayNamePreference? DisplayNamePreference { get; set; }
    public bool? ShowEnrollmentNumber { get; set; }
}
