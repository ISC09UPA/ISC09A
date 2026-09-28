namespace EasyMemories.Services;

public interface IQrCodeService
{
    /// <summary>Construye el deep link (easymemories://join/{joinCode}) que se codifica en el QR.</summary>
    string BuildJoinUrl(string joinCode);

    /// <summary>Genera el PNG del QR para un join code dado.</summary>
    byte[] GeneratePng(string joinCode);
}
