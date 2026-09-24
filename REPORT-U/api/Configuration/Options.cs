namespace ReportU.Configuration;

/// <summary>Opciones JWT. Se configuran por variables de entorno: Jwt__Key, Jwt__Issuer, Jwt__Audience, Jwt__ExpiresMinutes.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Clave HMAC-SHA256. Mínimo 32 caracteres. Nunca commitear la de producción.</summary>
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ReportU";
    public string Audience { get; set; } = "ReportU";
    public int ExpiresMinutes { get; set; } = 720;
}

/// <summary>
/// Opciones de almacenamiento de imágenes.
/// AzureBlob__ConnectionString vacío = almacenamiento local (./uploads) para desarrollo sin Azure.
/// </summary>
public class BlobOptions
{
    public const string SectionName = "AzureBlob";

    public string ConnectionString { get; set; } = string.Empty;
    public string Container { get; set; } = "reportu-media";
    public string LocalPath { get; set; } = "uploads";
}
