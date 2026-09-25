using ImageCards.Api.DTOs;

namespace ImageCards.Api.Services.Reviews;

/// <summary>Study sessions for the current user.</summary>
public interface IReviewService
{
    /// <summary>Cards with NextReviewAt &lt;= now that have a translation in <paramref name="language"/>, oldest first.</summary>
    Task<DueCardsResponseDto> GetDueCardsAsync(string language, int limit, CancellationToken cancellationToken);

    Task<ReviewStateDto> SubmitReviewAsync(SubmitReviewRequestDto request, CancellationToken cancellationToken);

    Task<ReviewStateDto> GetReviewStateAsync(Guid cardId, CancellationToken cancellationToken);
}
