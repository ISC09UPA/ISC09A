using System.ComponentModel.DataAnnotations;

namespace ImageCards.Api.Configuration;

public class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";

    /// <summary>Storage account connection string. Must contain an account key so SAS URLs can be signed.</summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$", ErrorMessage = "Invalid Azure container name.")]
    public string ContainerName { get; set; } = "card-images";

    /// <summary>Lifetime of the read-only SAS URLs returned to clients.</summary>
    [Range(1, 24 * 60)]
    public int SasExpiryMinutes { get; set; } = 60;
}
