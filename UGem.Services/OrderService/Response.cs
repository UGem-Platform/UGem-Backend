using UGem.Repositories.Entity;

namespace UGem.Services.OrderService;

public class Response
{
    public class GetOrderListResponse
    {
        public Guid OrderId { get; set; }
        public decimal FinalPrice { get; set; }
        public string DeliveryAddress { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
    }
    public class OrderResponse
    {
        public required string Name { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public required string Status { get; set; }
        public DateTimeOffset OrderedAt { get; set; }
        public required string Notes { get; set; }
        public required string DeliveryAddress { get; set; }
    }
    public class GetOrderDetailResponse
    {
        public required string Name {get; set;}
        public int Quantity {get; set;}
        public decimal UnitPrice {get; set;}
        public string? Notes {get; set;}
        public Guid FoodId { get; set; }
        public Guid OrderId { get; set; }

    }

}