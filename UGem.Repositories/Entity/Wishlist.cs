using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Wishlist : BaseEntity<Guid>, IAuditableEntity
{
    public Guid CustomerId { get; set; }
    public required Customer Customer { get; set; }
    
    public ICollection<WishlistDetail> Details { get; set; } = new List<WishlistDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}