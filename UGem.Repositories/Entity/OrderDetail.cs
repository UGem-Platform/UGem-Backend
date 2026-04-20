using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class OrderDetail: BaseEntity<Guid>, IAuditableEntity
{
    public required string Name {get; set;}
    public int Quantity {get; set;}
    public decimal UnitPrice {get; set;}
    public string? Notes {get; set;}
    
    public Guid OrderId { get; set; }
    public required Order Order { get; set; }
    
    public Guid FoodId { get; set; }
    public required Food Food { get; set; }
    
    public ReviewDetail? ReviewDetail { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
