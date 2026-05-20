using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Campaign : BaseEntity<Guid>, IAuditableEntity
{
    public required string Code { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public decimal DiscountValue { get; set; }

    public bool IsPercentage { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public int Quantity { get; set; }

    public int UsedCount { get; set; }

    public int MaxUsagePerUser { get; set; }

    public bool IsGlobal { get; set; }

    public bool IsNewUserOnly { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public Guid? MerchantId { get; set; }

    public Merchant? Merchant { get; set; }

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public ICollection<UserCampaignUsage> UserCampaignUsages { get; set; }
        = new List<UserCampaignUsage>();
}