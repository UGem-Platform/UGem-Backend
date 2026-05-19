using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class ReviewerWalletTransaction : BaseEntity<Guid>, IAuditableEntity
{
    public Guid ReviewerId { get; set; }
    public Reviewer Reviewer { get; set; } = null!;

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public decimal Amount { get; set; }
    
    /// <summary>
    /// Type of transaction. See <see cref="ReviewerWalletTransactionType"/> for possible values.
    /// </summary>
    public string Type { get; set; } = null!;
    
    public DateTimeOffset CreatedAtUtc { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Reason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
