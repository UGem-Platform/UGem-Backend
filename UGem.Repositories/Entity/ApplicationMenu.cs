using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class ApplicationMenu : BaseEntity<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    
    public required Guid ApplicationId { get; set; }
    public Application? Application { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}