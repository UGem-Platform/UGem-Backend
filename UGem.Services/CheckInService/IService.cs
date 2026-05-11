namespace UGem.Services.CheckInService;

public interface IService
{
    public byte[] GenerateQrCode(Request.GenerateQrCodeRequest request);
    
    public Task CreateCheckIn(Guid customerId, Guid merchantId);
    
    public Task FireCheckIn();

    
}