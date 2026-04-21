using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Review : BaseEntity<Guid>, IAuditableEntity
{
    public int Rating { get; set; } = 0;
    public string Content { get; set; } = "";
    public string? ImageUrl { get; set; }
    public bool IsVerified { get; set; } = false;
    
    public required Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public required Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; } = null!;
    
    public ICollection<ReviewDetail> ReviewDetails { get; set; } = new List<ReviewDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
