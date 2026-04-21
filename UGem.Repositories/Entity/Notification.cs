using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Notification: BaseEntity<Guid>, IAuditableEntity
{
    public required string Message { get; set; }
    public required string Title { get; set; }
    public bool IsRead { get; set; }
    public required string Type { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}