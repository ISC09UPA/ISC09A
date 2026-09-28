using System.ComponentModel.DataAnnotations;

namespace EasyMemories.DTOs;

public class UploadMemoryRequest
{
    [Required]
    public IFormFile Photo { get; set; } = null!;

    [MaxLength(80)]
    public string? GuestName { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}

public record MemoryResponse(
    int Id,
    string? GuestName,
    string? Comment,
    string PhotoUrl,
    DateTime CreatedAt);
