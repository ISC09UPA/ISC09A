namespace KIAN.DTOs;

public class TranslationDto
{
    public string LanguageCode { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
}

public class CardResponseDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<TranslationDto> Translations { get; set; } = new();
}