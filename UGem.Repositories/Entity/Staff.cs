using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Staff : BaseEntity<Guid>, IAuditableEntity
{
    public Guid UserId { get; set; }
    public required User User { get; set; }

    public DateTimeOffset HiredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}