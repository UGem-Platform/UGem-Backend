using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Reviewer : BaseEntity<Guid>, IAuditableEntity
{
    public required int Points { get; set; } = 0;
    public required string Rank { get; set; }
    public required float CommissionRate { get; set; } = 0;
    
    public Guid CustomerId { get; set; }
    public required Customer Customer { get; set; }
    
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}