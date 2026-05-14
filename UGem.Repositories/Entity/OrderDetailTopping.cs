using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class OrderDetailTopping : BaseEntity<Guid>, IAuditableEntity
{
    public required Guid OrderDetailId { get; set; }
    public OrderDetail OrderDetail { get; set; }

    public required Guid FoodToppingId { get; set; }
    public FoodTopping FoodTopping { get; set; }

    public required string Name { get; set; }

    public decimal Price { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}