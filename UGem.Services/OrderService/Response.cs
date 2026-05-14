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
        public required Guid OrderId { get; set; }
        public required Guid Id { get; set; }
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
        public Guid OrderId { get; set; }

        public Guid FoodId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public string? Notes { get; set; }

        public List<OrderDetailToppingResponse> Toppings { get; set; } = new();
    }
    
    public class CreateOrderResponse
    {
        public Guid OrderId { get; set; }
        public decimal TotalAmount{ get; set; }
        public string BankName { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public string Code { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
    }
    
    public class GetOrderBillResponse
    {
        public Guid OrderId { get; set; }

        public required string Name { get; set; }

        public required string Notes { get; set; }
 
        public required string PaymentMethod { get; set; }

        public DateTimeOffset OrderedAt { get; set; }

        public required string DeliveryAddress { get; set; }
        
        public decimal Discount { get; set; }

        public decimal FinalPrice { get; set; }

        public List<GetBillItemResponse> Items { get; set; } = new();
    }
        
    public class GetBillItemResponse
    {
        public required string Name { get; set; } 

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        
        public decimal SubTotal { get; set; }
 
        public List<OrderDetailToppingResponse> Toppings { get; set; } = new();
    }
    public class OrderDetailToppingResponse
    {
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }
    }
    
    public class UpdateBillResponse
    {
        public Guid OrderId { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public List<GetBillItemResponse> Items { get; set; } = new();
    }

}