namespace UGem.Services.OrderService;

public class Response
{
    public class GetOrderListResponse
    {
       public required string Name { get; set; }
       public decimal FinalPrice { get; set; }
       public required string DeliveryAddress { get; set; }
       public required string PaymentMethod { get; set; }
       public required string Status { get; set; }
       public required string CustomerName { get; set; }
       
    }
}