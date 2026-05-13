using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Food: BaseEntity<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
        
    public required Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; } = null!;
    
    public ICollection<CategoryDetail> CategoryDetails { get; set; } = new List<CategoryDetail>();

    public ICollection<FoodTopping> FoodToppings { get; set; } = new List<FoodTopping>();
    
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
