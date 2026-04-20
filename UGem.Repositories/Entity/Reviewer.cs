using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Reviewer : BaseEntity<Guid>, IAuditableEntity
{
    public required int Points { get; set; } = 0;
    public required string Rank { get; set; }
    public decimal CommissionRate { get; set; }
    
    public Guid CustomerId { get; set; }
    public required Customer Customer { get; set; }
    
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public ICollection<AffiliateLink> AffiliateLinks { get; set; } = new List<AffiliateLink>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
