using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Order: BaseEntity<Guid>, IAuditableEntity
{
   
    public required string Name { get; set; }
    public double Discount { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal ReviewerFee { get; set; }
    public decimal PlatFormFee { get; set; }
    public required string Status { get; set; }
    public required string PaymentMethod { get; set; }
    public DateTime OrderdAt { get; set; }
    public required string Notes { get; set; }
    public required string DeliveryAddress { get; set; }
    
    public Guid CustomerId { get; set; }
    public required Customer Customer { get; set; }
    
    public Guid? ReviewId { get; set; }
    public Review? Review { get; set; }
    
    public Guid? AffiliateLinkId { get; set; }
    public AffiliateLink? AffiliateLink { get; set; }
    
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}