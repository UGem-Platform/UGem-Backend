using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class ReviewerApplication : BaseEntity<Guid>, IAuditableEntity
{
    public required string Status { get; set; } = "Pending";
    public required string Motivation { get; set; }
    public string? Experience { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TiktokUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? OtherSocialUrl { get; set; }

    public required Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}