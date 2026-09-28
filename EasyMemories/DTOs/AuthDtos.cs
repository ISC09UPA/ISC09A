using System.ComponentModel.DataAnnotations;

namespace EasyMemories.DTOs;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string DisplayName);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(string Token, DateTimeOffset ExpiresAt, string DisplayName, string Email);
