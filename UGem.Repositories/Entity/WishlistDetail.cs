using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class WishlistDetail : BaseEntity<Guid>, IAuditableEntity
{
    public Guid WishlistId { get; set; }
    public required Wishlist Wishlist { get; set; }
    
    public Guid MerchantId { get; set; }
    public required Merchant Merchant { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}