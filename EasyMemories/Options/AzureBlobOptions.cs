namespace EasyMemories.Options;

public class AzureBlobOptions
{
    public const string SectionName = "AzureBlobStorage";

    public string ConnectionString { get; set; } = string.Empty;

    public string ContainerName { get; set; } = "memories";

    /// <summary>Minutos de validez de las URLs firmadas (SAS) devueltas al cliente.</summary>
    public int SasExpiryMinutes { get; set; } = 60 * 24 * 7;
}
