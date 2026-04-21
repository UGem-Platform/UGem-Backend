using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class WishlistDetail : BaseEntity<Guid>, IAuditableEntity
{
    
    public required Guid WishlistId { get; set; }
    public Wishlist Wishlist { get; set; }
    
    public required Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}