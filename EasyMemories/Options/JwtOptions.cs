namespace EasyMemories.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "EasyMemories";

    public string Audience { get; set; } = "EasyMemories.Clients";

    public int ExpiryMinutes { get; set; } = 60 * 24;
}
