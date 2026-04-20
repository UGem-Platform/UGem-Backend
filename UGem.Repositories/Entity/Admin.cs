using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Admin : BaseEntity<Guid>, IAuditableEntity
{
    public Guid UserId { get; set; }
    public required User User { get; set; }

    public string? Permissions { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}