using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class ReviewDetail : BaseEntity<Guid>, IAuditableEntity
{
    public string? DetailContent { get; set; } = "";
    public int Rating { get; set; } = 0;
    
    public required Guid ReviewId { get; set; }
    public Review Review { get; set; }
    
    public required Guid OrderDetailId { get; set; }
    public OrderDetail OrderDetail { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
