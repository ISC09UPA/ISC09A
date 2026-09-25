namespace ImageCards.Api.Models;

/// <summary>Language codes (ISO 639-1) accepted by the API. Add new codes here.</summary>
public static class SupportedLanguages
{
    public const string Default = "en";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.Ordinal) { "en", "es", "fr", "de", "it", "pt" };

    public static bool IsSupported(string? code) => code is not null && All.Contains(Normalize(code));

    public static string Normalize(string code) => code.Trim().ToLowerInvariant();
}
