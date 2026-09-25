using ImageCards.Api.DTOs;
using ImageCards.Api.Services.Reviews;
using Microsoft.AspNetCore.Mvc;

namespace ImageCards.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController(IReviewService reviewService) : ControllerBase
{
    /// <summary>Cards due for review (NextReviewAt &lt;= now) in the given language.</summary>
    [HttpGet("due")]
    [ProducesResponseType<DueCardsResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<DueCardsResponseDto> GetDueCards(
        [FromQuery] DueCardsQueryParameters query, CancellationToken cancellationToken) =>
        reviewService.GetDueCardsAsync(query.Language, query.Limit, cancellationToken);

    /// <summary>Records how well a card was remembered and schedules its next review.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<ReviewStateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ReviewStateDto> SubmitReview(
        [FromBody] SubmitReviewRequestDto request, CancellationToken cancellationToken) =>
        reviewService.SubmitReviewAsync(request, cancellationToken);

    /// <summary>Current spaced repetition state of a card.</summary>
    [HttpGet("{cardId:guid}")]
    [ProducesResponseType<ReviewStateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ReviewStateDto> GetReviewState(Guid cardId, CancellationToken cancellationToken) =>
        reviewService.GetReviewStateAsync(cardId, cancellationToken);
}
