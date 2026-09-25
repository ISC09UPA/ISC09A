using System.ComponentModel.DataAnnotations;
using ImageCards.Api.Data;
using ImageCards.Api.DTOs.Validation;
using ImageCards.Api.Models;

namespace ImageCards.Api.DTOs;

public sealed record TranslationDto(string Language, string TranslatedText, string? ExampleSentence);

/// <summary>A card as returned by the cards endpoints. <c>ImageUrl</c> is a temporary SAS URL.</summary>
public sealed record CardResponseDto(
    Guid Id,
    string ImageUrl,
    DateTime CreatedAt,
    IReadOnlyList<TranslationDto> Translations);

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public sealed class TranslationRequestDto
{
    [Required]
    [LanguageCode]
    public string? Language { get; init; }

    [Required(AllowEmptyStrings = false)]
    [MaxLength(FieldLengths.TranslatedText)]
    public string? TranslatedText { get; init; }

    [MaxLength(FieldLengths.ExampleSentence)]
    public string? ExampleSentence { get; init; }
}

/// <summary>Shared rules for create and update requests.</summary>
public abstract class CardRequestDtoBase : IValidatableObject
{
    public const int MaxTranslations = 20;

    [Required]
    [MinLength(1, ErrorMessage = "At least one translation is required.")]
    [MaxLength(MaxTranslations)]
    public List<TranslationRequestDto> Translations { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var duplicated = Translations
            .Where(t => t.Language is not null)
            .GroupBy(t => SupportedLanguages.Normalize(t.Language!))
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicated.Count > 0)
        {
            yield return new ValidationResult(
                $"Duplicated translation languages: {string.Join(", ", duplicated)}.",
                [nameof(Translations)]);
        }
    }
}

/// <summary>Sent as multipart/form-data: <c>image</c> plus <c>translations[i].language</c>, etc.</summary>
public sealed class CreateCardRequestDto : CardRequestDtoBase
{
    [Required]
    public IFormFile? Image { get; init; }
}

/// <summary>Replaces all translations. The image is replaced only if a new one is sent.</summary>
public sealed class UpdateCardRequestDto : CardRequestDtoBase
{
    public IFormFile? Image { get; init; }
}

public sealed class CardQueryParameters
{
    public const int MaxPageSize = 100;

    /// <summary>When set, only cards with a translation in this language are returned, with only that translation.</summary>
    [LanguageCode]
    public string? Language { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize { get; init; } = 20;
}
