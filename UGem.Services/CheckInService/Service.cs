using QRCoder;

namespace UGem.Services.CheckInService;

public class Service
{
    public byte[] GenerateQrCode(string text)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}