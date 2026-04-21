using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class CategoryDetail: BaseEntity<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public required string ImgUrl { get; set; }
    public required string Description { get; set; }
    
    public required Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    public required Guid FoodId { get; set; }
    public Food Food { get; set; } = null!;
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
