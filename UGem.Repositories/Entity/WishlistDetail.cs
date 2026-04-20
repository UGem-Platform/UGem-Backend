using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class WishlistDetail : BaseEntity<Guid>, IAuditableEntity
{
    public required string Name { get; set; } = "";
    public required string LogoUrl { get; set; } = "";
    public decimal Rating { get; set; }
    
    public Guid WishlistId { get; set; }
    public required Wishlist Wishlist { get; set; }
    
    public Guid MerchantId { get; set; }
    public required Merchant Merchant { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}