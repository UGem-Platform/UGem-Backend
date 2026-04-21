using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Category: BaseEntity<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Slug { get; set; }
    public bool IsActive { get; set; }
    
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    
    public ICollection<CategoryDetail> CategoryDetails { get; set; } = new List<CategoryDetail>();
    public ICollection<Category> Children { get; set; } = new List<Category>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

}