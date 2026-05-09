using UGem.Repositories.Entity;

namespace UGem.Services.OrderService;

public class Request
{
    public enum OrderStatus
    {
        Pending,
        Accepted,
        Rejected,
        Completed,
        NotReceived,
        Expired
    }

    public class CreateOrderRequest
    {
        public required string Name { get; set; }
        public required string PaymentMethod { get; set; }
        public required string Notes { get; set; }
        public required string DeliveryAddress { get; set; }
        public required List<FoodService.Request.FoodOrderRequest> Foods { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public required OrderStatus Status { get; set; }
        public string? Reason { get; set; }
    }

    public class ReasonRejectRequest
    {
        public string? Reason { get; set; }
        public Guid OrderId { get; set; }
    }
    public class ConfirmOrderRequest
    {
        public Guid OrderId { get; set; }
    }
    
    
    public class SepayWebhookRequest
    {
        public string? Gateway { get; set; }
        public string? TransactionDate { get; set; }
        public string? AccountNumber { get; set; }
        public string? SubAccount { get; set; }
        public string? Code { get; set; }
        public string? Content { get; set; }
        public string? TransferType { get; set; }
        public string? Description { get; set; }
        public decimal TransferAmount { get; set; }
        public string? ReferenceCode { get; set; }
        public decimal Accumulated { get; set; }
        public long Id { get; set; }
    }
}
