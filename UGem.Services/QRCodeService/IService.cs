namespace UGem.Services.QRCodeService;

public interface IService
{
    byte[] GenerateQrCode(string text);
}