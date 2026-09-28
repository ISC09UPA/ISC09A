using System.ComponentModel.DataAnnotations;

namespace EasyMemories.DTOs;

public record CreateSpaceRequest(
    [Required, MaxLength(120)] string Name,
    [MaxLength(500)] string? Description);

public record SpaceResponse(
    int Id,
    string Name,
    string? Description,
    string JoinCode,
    string JoinUrl,
    bool IsActive,
    DateTime CreatedAt,
    int MemoryCount);

/// <summary>Lo que ve un invitado antes de subir nada, para confirmar a qué fiesta se está uniendo.</summary>
public record SpacePublicInfo(string Name, string? Description, bool IsActive);
