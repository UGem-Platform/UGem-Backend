using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class OrderDetailTopping : BaseEntity<Guid>, IAuditableEntity
{
    public Guid OrderDetailId { get; set; }
    public required OrderDetail OrderDetail { get; set; }

    public Guid FoodToppingId { get; set; }
    public required FoodTopping FoodTopping { get; set; }

    public required string Name { get; set; }

    public decimal Price { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}