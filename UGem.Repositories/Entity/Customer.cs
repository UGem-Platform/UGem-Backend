using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Customer : BaseEntity<Guid>, IAuditableEntity
{
    public int TotalCheckIns { get; set; }
    
    public Guid ReviewerId { get; set; }
    public Reviewer? Reviewer { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}