using EasyMemories.Data;
using EasyMemories.DTOs;
using EasyMemories.Models;
using EasyMemories.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyMemories.Controllers;

/// <summary>
/// Endpoints públicos para invitados: el acceso al "espacio" está controlado únicamente
/// por conocer el JoinCode (lo que se obtiene al escanear el QR), sin login.
/// </summary>
[ApiController]
[Route("api/join")]
public class JoinController : ControllerBase
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/heic"
    };

    private const long MaxPhotoBytes = 15 * 1024 * 1024; // 15 MB

    private readonly AppDbContext _db;
    private readonly IBlobStorageService _blobStorageService;

    public JoinController(AppDbContext db, IBlobStorageService blobStorageService)
    {
        _db = db;
        _blobStorageService = blobStorageService;
    }

    [HttpGet("{joinCode}")]
    public async Task<ActionResult<SpacePublicInfo>> GetSpaceInfo(string joinCode)
    {
        var space = await FindActiveSpaceAsync(joinCode);
        if (space is null) return NotFound(new { message = "Espacio no encontrado o inactivo." });

        return Ok(new SpacePublicInfo(space.Name, space.Description, space.IsActive));
    }

    [HttpGet("{joinCode}/memories")]
    public async Task<ActionResult<List<MemoryResponse>>> GetMemories(string joinCode)
    {
        var space = await FindActiveSpaceAsync(joinCode);
        if (space is null) return NotFound(new { message = "Espacio no encontrado o inactivo." });

        var memories = await _db.Memories
            .Where(m => m.SpaceId == space.Id)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(memories.Select(m => new MemoryResponse(
            m.Id, m.GuestName, m.Comment, _blobStorageService.GetReadUrl(m.BlobName), m.CreatedAt)).ToList());
    }

    [HttpPost("{joinCode}/memories")]
    [RequestSizeLimit(MaxPhotoBytes)]
    public async Task<ActionResult<MemoryResponse>> UploadMemory(string joinCode, [FromForm] UploadMemoryRequest request)
    {
        var space = await FindActiveSpaceAsync(joinCode);
        if (space is null) return NotFound(new { message = "Espacio no encontrado o inactivo." });

        if (request.Photo.Length == 0)
        {
            return BadRequest(new { message = "La foto está vacía." });
        }

        if (request.Photo.Length > MaxPhotoBytes)
        {
            return BadRequest(new { message = "La foto supera el tamaño máximo permitido (15 MB)." });
        }

        if (!AllowedContentTypes.Contains(request.Photo.ContentType))
        {
            return BadRequest(new { message = "Formato de imagen no soportado." });
        }

        await using var stream = request.Photo.OpenReadStream();
        var blobName = await _blobStorageService.UploadPhotoAsync(stream, request.Photo.ContentType);

        var memory = new Memory
        {
            SpaceId = space.Id,
            GuestName = request.GuestName,
            Comment = request.Comment,
            BlobName = blobName,
            ContentType = request.Photo.ContentType
        };

        _db.Memories.Add(memory);
        await _db.SaveChangesAsync();

        return Ok(new MemoryResponse(
            memory.Id, memory.GuestName, memory.Comment, _blobStorageService.GetReadUrl(memory.BlobName), memory.CreatedAt));
    }

    private Task<Space?> FindActiveSpaceAsync(string joinCode) =>
        _db.Spaces.FirstOrDefaultAsync(s => s.JoinCode == joinCode && s.IsActive);
}
