using ImageCards.Api.DTOs;

namespace ImageCards.Api.Services.Cards;

/// <summary>Card management for the current user.</summary>
public interface ICardService
{
    Task<PagedResponse<CardResponseDto>> GetCardsAsync(CardQueryParameters query, CancellationToken cancellationToken);

    /// <summary>Gets a card. When <c>language</c> is set, only that translation is returned (404 if missing).</summary>
    Task<CardResponseDto> GetCardAsync(Guid id, string? language, CancellationToken cancellationToken);

    Task<TranslationDto> GetTranslationAsync(Guid id, string language, CancellationToken cancellationToken);

    Task<CardResponseDto> CreateCardAsync(CreateCardRequestDto request, CancellationToken cancellationToken);

    Task<CardResponseDto> UpdateCardAsync(Guid id, UpdateCardRequestDto request, CancellationToken cancellationToken);

    Task DeleteCardAsync(Guid id, CancellationToken cancellationToken);
}
