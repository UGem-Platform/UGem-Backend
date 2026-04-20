using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class CategoryDetail: BaseEntity<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public required string ImgUrl { get; set; }
    public required string Description { get; set; }
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
    
    public Guid FoodId { get; set; }
    public Food? Food { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}