using System.ComponentModel.DataAnnotations;

namespace ReportU.Models;

/// <summary>Estudiante registrado. Rol único del sistema (Student).</summary>
public class User
{
    public Guid Id { get; set; }

    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Hash BCrypt de la contraseña. Nunca se expone en respuestas.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Career { get; set; } = string.Empty;

    [MaxLength(30)]
    public string EnrollmentNumber { get; set; } = string.Empty;

    public DisplayNamePreference DisplayNamePreference { get; set; } = DisplayNamePreference.Username;

    public bool ShowEnrollmentNumber { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostSupport> Supports { get; set; } = new List<PostSupport>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
