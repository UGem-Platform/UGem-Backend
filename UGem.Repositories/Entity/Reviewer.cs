using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Reviewer : BaseEntity<Guid>, IAuditableEntity
{
    public const string BronzeRank = "Bronze";
    public const string SilverRank = "Silver";
    public const string GoldRank = "Gold";
    public const string DiamondRank = "Diamond";

    public required int Points { get; set; } = 0;
    public required string Rank { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal Balance { get; set; } = 0;

    public required Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public ICollection<AffiliateLink> AffiliateLinks { get; set; } = new List<AffiliateLink>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void SyncRankWithPoints()
    {
        Rank = CalculateRank(Points);
    }

    public static string CalculateRank(int points)
    {
        if (points >= 100)
        {
            return DiamondRank;
        }

        if (points >= 50)
        {
            return GoldRank;
        }

        if (points >= 20)
        {
            return SilverRank;
        }

        return BronzeRank;
    }
}
