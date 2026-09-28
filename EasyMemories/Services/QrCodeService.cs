using QRCoder;

namespace EasyMemories.Services;

public class QrCodeService : IQrCodeService
{
    // Debe coincidir con el "scheme" registrado en app.json de la app Expo/React Native.
    private const string AppScheme = "easymemories";

    public string BuildJoinUrl(string joinCode) => $"{AppScheme}://join/{joinCode}";

    public byte[] GeneratePng(string joinCode)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(BuildJoinUrl(joinCode), QRCodeGenerator.ECCLevel.Q);
        var pngQrCode = new PngByteQRCode(data);
        return pngQrCode.GetGraphic(20);
    }
}
