using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class FoodTopping : BaseEntity<Guid>, IAuditableEntity
{
    public required Guid FoodId { get; set; }
    public Food Food { get; set; }

    public required string Name { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
