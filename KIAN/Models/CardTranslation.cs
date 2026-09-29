namespace KIAN.Models;

public class CardTranslation
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }

    public Card? Card { get; set; }
}