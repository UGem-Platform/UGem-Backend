using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class UserCampaignUsage : BaseEntity<Guid>, IAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; }

    public int UsedCount { get; set; }

    public DateTimeOffset LastUsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}