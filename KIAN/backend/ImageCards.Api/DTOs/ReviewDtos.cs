using System.ComponentModel.DataAnnotations;
using ImageCards.Api.DTOs.Validation;
using ImageCards.Api.Models;

namespace ImageCards.Api.DTOs;

/// <summary>A card ready to be studied in one language.</summary>
public sealed record FlashcardDto(
    Guid Id,
    string ImageUrl,
    string Translation,
    string Language,
    string? ExampleSentence,
    DateTime NextReviewAt);

public sealed record DueCardsResponseDto(int TotalDue, IReadOnlyList<FlashcardDto> Cards);

public sealed record ReviewStateDto(
    Guid CardId,
    DateTime NextReviewAt,
    DateTime? LastReviewedAt,
    int IntervalDays,
    double EaseFactor,
    int Repetitions,
    int Lapses);

public sealed class SubmitReviewRequestDto
{
    [Required]
    public Guid? CardId { get; init; }

    [Required]
    public ReviewRating? Rating { get; init; }
}

public sealed class DueCardsQueryParameters
{
    public const int MaxLimit = 200;

    [LanguageCode]
    public string Language { get; init; } = SupportedLanguages.Default;

    [Range(1, MaxLimit)]
    public int Limit { get; init; } = 50;
}
