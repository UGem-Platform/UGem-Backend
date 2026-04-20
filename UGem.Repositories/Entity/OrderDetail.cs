using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class OrderDetail: BaseEntity<Guid>, IAuditableEntity
{
    public required string Name {get; set;}
    public double Quantity {get; set;}
    public double UnitPrice {get; set;}
    public string? Notes {get; set;}
    
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    
    public Guid FoodId { get; set; }
    public Food Food { get; set; }
    
    public Guid ReviewDetailId { get; set; }
    public ReviewDetail? ReviewDetail { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}