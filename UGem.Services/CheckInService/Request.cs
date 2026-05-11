namespace UGem.Services.CheckInService;

public class Request
{
    public class CreateCheckInForQr
    {
        public Guid OrderId { get; set; }
    }
}