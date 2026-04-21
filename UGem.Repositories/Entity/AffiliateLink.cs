using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class AffiliateLink : BaseEntity<Guid>, IAuditableEntity
{
    public required string LinkCode { get; set; }
 
    public int ClickCount { get; set; } = 0;
    public int OrderCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    public required Guid ReviewerId { get; set; }
    public Reviewer Reviewer { get; set; } = null!;
    
    public required Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; } = null!;
    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
 
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
        
