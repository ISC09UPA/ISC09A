namespace ImageCards.Api.Data;

/// <summary>Column lengths shared by the EF configuration and DTO validation.</summary>
public static class FieldLengths
{
    public const int UserName = 100;
    public const int Email = 256;
    public const int BlobName = 200;
    public const int LanguageCode = 2;
    public const int TranslatedText = 200;
    public const int ExampleSentence = 500;
}
