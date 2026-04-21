using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class CheckIn : BaseEntity<Guid>, IAuditableEntity
{
    public required Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; } = null!;
    
    public required Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
