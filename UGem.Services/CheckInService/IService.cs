namespace UGem.Services.CheckInService;

public interface IService
{
    public Task<byte[]> GenerateQrCode(Request.GenerateQrCodeRequest request);
    
    public Task CreateCheckIn(Guid customerId, Guid merchantId);
    
    public Task CreateCheckInForQr(Request.CreateCheckInForQr request);

    
}