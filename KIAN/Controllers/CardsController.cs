using KIAN.DTOs;
using KIAN.Services;
using Microsoft.AspNetCore.Mvc;

namespace KIAN.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB

    private readonly CardService _cards;

    public CardsController(CardService cards) => _cards = cards;

    // GET /api/cards  |  GET /api/cards?language=en
    [HttpGet]
    public async Task<ActionResult<List<CardResponseDto>>> GetAll([FromQuery] string? language)
        => await _cards.GetAllAsync(language);

    // GET /api/cards/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CardResponseDto>> GetById(Guid id)
    {
        var card = await _cards.GetByIdAsync(id);
        return card is null ? NotFound() : card;
    }

    // POST /api/cards  (multipart/form-data)
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CardResponseDto>> Create([FromForm] CreateCardDto dto)
    {
        if (dto.Image.Length == 0 || dto.Image.Length > MaxImageBytes)
            return BadRequest("La imagen debe pesar entre 1 byte y 5 MB.");

        if (!AllowedTypes.Contains(dto.Image.ContentType))
            return BadRequest("Solo se permiten imágenes JPG, PNG o WEBP.");

        var card = await _cards.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = card.Id }, card);
    }

    // POST /api/cards/{id}/translations
    [HttpPost("{id:guid}/translations")]
    public async Task<ActionResult<CardResponseDto>> AddTranslation(Guid id, AddTranslationDto dto)
    {
        try
        {
            return await _cards.AddTranslationAsync(id, dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}