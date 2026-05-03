namespace UGem.Services.CheckInService;

public interface IService
{
    byte[] GenerateQrCode(string text);
}