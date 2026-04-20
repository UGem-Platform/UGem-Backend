using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Customer : BaseEntity<Guid>, IAuditableEntity
{
    public int TotalCheckIns { get; set; }
    
    public Guid UserId { get; set; }
    public required User User { get; set; }
    
    public Reviewer? Reviewer { get; set; }
    
    public Guid WishListId { get; set; }
    public Wishlist? WishList { get; set; }
    
    public ICollection<Order> Orders { get; set; } =  new List<Order>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}