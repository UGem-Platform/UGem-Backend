namespace UGem.Services.CheckInService;

public class Response
{
    public class CurrentCheckInResponse
    {
        public Guid CheckInId { get; set; }
        public Guid MerchantId { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public DateTimeOffset ? CreatedAt { get; set; }
    }
}