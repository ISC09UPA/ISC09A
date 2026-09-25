using ImageCards.Api.DTOs;
using ImageCards.Api.DTOs.Validation;
using ImageCards.Api.Services.Cards;
using Microsoft.AspNetCore.Mvc;

namespace ImageCards.Api.Controllers;

[ApiController]
[Route("api/cards")]
public sealed class CardsController(ICardService cardService) : ControllerBase
{
    /// <summary>Lists the current user's cards, newest first.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResponse<CardResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<PagedResponse<CardResponseDto>> GetCards(
        [FromQuery] CardQueryParameters query, CancellationToken cancellationToken) =>
        cardService.GetCardsAsync(query, cancellationToken);

    [HttpGet("{id:guid}", Name = nameof(GetCard))]
    [ProducesResponseType<CardResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<CardResponseDto> GetCard(
        Guid id, [FromQuery, LanguageCode] string? language, CancellationToken cancellationToken) =>
        cardService.GetCardAsync(id, language, cancellationToken);

    [HttpGet("{id:guid}/translations/{language}")]
    [ProducesResponseType<TranslationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<TranslationDto> GetTranslation(
        Guid id, [LanguageCode] string language, CancellationToken cancellationToken) =>
        cardService.GetTranslationAsync(id, language, cancellationToken);

    /// <summary>Creates a card from an image (JPEG, PNG or WebP) and at least one translation.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<CardResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CardResponseDto>> CreateCard(
        [FromForm] CreateCardRequestDto request, CancellationToken cancellationToken)
    {
        var card = await cardService.CreateCardAsync(request, cancellationToken);
        return CreatedAtRoute(nameof(GetCard), new { id = card.Id }, card);
    }

    /// <summary>Replaces the card translations and, optionally, its image.</summary>
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<CardResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<CardResponseDto> UpdateCard(
        Guid id, [FromForm] UpdateCardRequestDto request, CancellationToken cancellationToken) =>
        cardService.UpdateCardAsync(id, request, cancellationToken);

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCard(Guid id, CancellationToken cancellationToken)
    {
        await cardService.DeleteCardAsync(id, cancellationToken);
        return NoContent();
    }
}
