namespace UGem.Services.CheckInService;

public class Request
{
    public class GenerateQrCodeRequest
    {
        public Guid MerchantId { get; set; }
    }
}