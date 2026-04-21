using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Application: BaseEntity<Guid>, IAuditableEntity
{
    public required string Type { get; set; }
    public required string Status { get; set; }
    public DateTime ReviewedAt { get; set; }
    
    public required Guid UserId { get; set; }
    public User? User { get; set; }

    public ICollection<ApplicationMenu> ApplicationMenus { get; set; } = new List<ApplicationMenu>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
