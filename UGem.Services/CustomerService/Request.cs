namespace UGem.Services.CustomerService;

public class Request
{
    public class ConfirmOrderRequest
    {
        public Guid OrderId { get; set; }
    }
}