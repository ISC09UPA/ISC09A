namespace ImageCards.Api.Models;

public class CardTranslation
{
    public Guid Id { get; set; }

    public Guid CardId { get; set; }

    public Card? Card { get; set; }

    /// <summary>ISO 639-1 language code, lower case (e.g. "en").</summary>
    public required string LanguageCode { get; set; }

    public required string TranslatedText { get; set; }

    public string? ExampleSentence { get; set; }
}
