using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Order: BaseEntity<Guid>, IAuditableEntity
{
   
    public required string Name { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalPrice { get; set; }
    
    public decimal ReviewerFee { get; set; }
    public decimal PlatformFee { get; set; }
    public required string Status { get; set; }
    public required string PaymentMethod { get; set; }
    
    public DateTimeOffset OrderedAt { get; set; }
    public required string Notes { get; set; }
    public string? RejectionReason { get; set; }
    public string OrderType { get; set; } = "Offline";
    public string PaymentStatus { get; set; } = "Unpaid";
    public string? DeliveryAddress { get; set; }
    
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public Review? Review { get; set; }
    
    public Guid? AffiliateLinkId { get; set; }
    public AffiliateLink? AffiliateLink { get; set; }
    
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}