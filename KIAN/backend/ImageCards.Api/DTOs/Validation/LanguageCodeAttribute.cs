using System.ComponentModel.DataAnnotations;
using ImageCards.Api.Models;

namespace ImageCards.Api.DTOs.Validation;

/// <summary>Accepts null (combine with [Required] if needed) or a code listed in <see cref="SupportedLanguages"/>.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class LanguageCodeAttribute : ValidationAttribute
{
    public LanguageCodeAttribute()
        : base($"Unsupported language code. Supported: {string.Join(", ", SupportedLanguages.All)}.")
    {
    }

    public override bool IsValid(object? value) =>
        value is null || (value is string code && SupportedLanguages.IsSupported(code));
}
