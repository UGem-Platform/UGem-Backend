using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Application: BaseEntity<Guid>, IAuditableEntity
{
    public required string Type { get; set; }
    public required string Status { get; set; }
    public DateTime ReviewedAt { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}