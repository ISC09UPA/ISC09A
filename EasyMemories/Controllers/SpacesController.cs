using System.Security.Claims;
using EasyMemories.Data;
using EasyMemories.DTOs;
using EasyMemories.Models;
using EasyMemories.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyMemories.Controllers;

[ApiController]
[Route("api/spaces")]
[Authorize]
public class SpacesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IQrCodeService _qrCodeService;
    private readonly IBlobStorageService _blobStorageService;

    public SpacesController(AppDbContext db, IQrCodeService qrCodeService, IBlobStorageService blobStorageService)
    {
        _db = db;
        _qrCodeService = qrCodeService;
        _blobStorageService = blobStorageService;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Token sin NameIdentifier.");

    [HttpPost]
    public async Task<ActionResult<SpaceResponse>> Create(CreateSpaceRequest request)
    {
        var space = new Space
        {
            Name = request.Name,
            Description = request.Description,
            OwnerUserId = CurrentUserId,
            JoinCode = await GenerateUniqueJoinCodeAsync()
        };

        _db.Spaces.Add(space);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = space.Id }, ToResponse(space, 0));
    }

    [HttpGet]
    public async Task<ActionResult<List<SpaceResponse>>> GetMine()
    {
        var spaces = await _db.Spaces
            .Where(s => s.OwnerUserId == CurrentUserId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new { Space = s, Count = s.Memories.Count })
            .ToListAsync();

        return Ok(spaces.Select(x => ToResponse(x.Space, x.Count)).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpaceResponse>> GetById(int id)
    {
        var space = await _db.Spaces
            .Where(s => s.Id == id && s.OwnerUserId == CurrentUserId)
            .Select(s => new { Space = s, Count = s.Memories.Count })
            .FirstOrDefaultAsync();

        if (space is null) return NotFound();

        return Ok(ToResponse(space.Space, space.Count));
    }

    [HttpGet("{id:int}/qrcode")]
    public async Task<IActionResult> GetQrCode(int id)
    {
        var space = await _db.Spaces.FirstOrDefaultAsync(s => s.Id == id && s.OwnerUserId == CurrentUserId);
        if (space is null) return NotFound();

        var png = _qrCodeService.GeneratePng(space.JoinCode);
        return File(png, "image/png");
    }

    [HttpGet("{id:int}/memories")]
    public async Task<ActionResult<List<MemoryResponse>>> GetMemories(int id)
    {
        var space = await _db.Spaces.FirstOrDefaultAsync(s => s.Id == id && s.OwnerUserId == CurrentUserId);
        if (space is null) return NotFound();

        var memories = await _db.Memories
            .Where(m => m.SpaceId == id)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(memories.Select(ToMemoryResponse).ToList());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var space = await _db.Spaces.FirstOrDefaultAsync(s => s.Id == id && s.OwnerUserId == CurrentUserId);
        if (space is null) return NotFound();

        var blobNames = await _db.Memories.Where(m => m.SpaceId == id).Select(m => m.BlobName).ToListAsync();
        foreach (var blobName in blobNames)
        {
            await _blobStorageService.DeleteAsync(blobName);
        }

        _db.Spaces.Remove(space);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string> GenerateUniqueJoinCodeAsync()
    {
        string code;
        do
        {
            code = JoinCodeGenerator.Generate();
        } while (await _db.Spaces.AnyAsync(s => s.JoinCode == code));

        return code;
    }

    private SpaceResponse ToResponse(Space space, int memoryCount) => new(
        space.Id,
        space.Name,
        space.Description,
        space.JoinCode,
        _qrCodeService.BuildJoinUrl(space.JoinCode),
        space.IsActive,
        space.CreatedAt,
        memoryCount);

    private MemoryResponse ToMemoryResponse(Memory memory) => new(
        memory.Id,
        memory.GuestName,
        memory.Comment,
        _blobStorageService.GetReadUrl(memory.BlobName),
        memory.CreatedAt);
}
