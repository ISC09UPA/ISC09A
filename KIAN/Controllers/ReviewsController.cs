using KIAN.DTOs;
using KIAN.Services;
using Microsoft.AspNetCore.Mvc;

namespace KIAN.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly CardService _cards;

    public ReviewsController(CardService cards) => _cards = cards;

    // GET /api/reviews/due?language=en&limit=20
    [HttpGet("due")]
    public async Task<ActionResult<List<CardResponseDto>>> GetDue(
        [FromQuery] string? language,
        [FromQuery] int limit = 20)
    {
        limit = Math.Clamp(limit, 1, 100);
        return await _cards.GetDueAsync(language, limit);
    }

    // POST /api/reviews
    [HttpPost]
    public async Task<IActionResult> Register(ReviewRequestDto dto)
    {
        try
        {
            await _cards.RegisterReviewAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}