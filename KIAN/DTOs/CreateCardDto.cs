using System.ComponentModel.DataAnnotations;

namespace KIAN.DTOs;

public class CreateCardDto
{
    [Required]
    public IFormFile Image { get; set; } = default!;

    [Required, StringLength(10)]
    public string LanguageCode { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string TranslatedText { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? ExampleSentence { get; set; }
}