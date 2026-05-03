namespace UGem.Services.CheckInService;

public interface IService
{
    public byte[] GenerateQrCode(string text);
    
    public Task CreateCheckIn(Guid customerId, Guid merchantId);
    
}