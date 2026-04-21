using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Wishlist : BaseEntity<Guid>, IAuditableEntity
{
    public required Guid CustomerId { get; set; }
    public Customer Customer { get; set; }
    
    public ICollection<WishlistDetail> WishlistDetails { get; set; } = new List<WishlistDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
