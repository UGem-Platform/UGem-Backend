using UGem.Repositories.Entity;

namespace UGem.Services.OrderService;

public class Request
{
    public class CreateOrderRequest
    {
        /*public Guid customerId { get; set; }*/
        public required string Name { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal ReviewerFee { get; set; }
        public decimal PlatformFee { get; set; }
        public required string Status { get; set; }
        public required string PaymentMethod { get; set; }
        public DateTimeOffset OrderedAt { get; set; }
        public required string Notes { get; set; }
        public required string DeliveryAddress { get; set; }
        
        public List<FoodService.Request.FoodOrderRequest> Foods { get; set; }
    }

    public class ReasonRejectRequest
    {
        public string? Reason { get; set; }
        public Guid OrderId { get; set; }
    }
    

}