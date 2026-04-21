using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class CheckIn : BaseEntity<Guid>, IAuditableEntity
{
    public required Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    
    public required Guid CustomerId { get; set; }
    public Customer Customer { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
