using System.ComponentModel.DataAnnotations;
using ImageCards.Api.Data;

namespace ImageCards.Api.Configuration;

/// <summary>
/// Local development user. Only used when the app runs in the Development environment,
/// until real authentication (JWT / Entra ID) is added.
/// </summary>
public class DevelopmentUserOptions
{
    public const string SectionName = "DevelopmentUser";

    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(FieldLengths.UserName)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(FieldLengths.Email)]
    public string Email { get; set; } = string.Empty;
}
