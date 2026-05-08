using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Application: BaseEntity<Guid>, IAuditableEntity
{
    public required string Type { get; set; }
    public required string Status { get; set; }
    public string? Note { get; set; }
    public DateTime ReviewedAt { get; set; }
    
    public required string Name { get; set; }
    public required  string Description { get; set; }
    public string? RestaurantType { get; set; }
    public string? MainDishType { get; set; }
    public decimal? PriceRange { get; set; }
    public required  string Email { get; set; }
    public required  string Phone { get; set; }
    public required string LogoUrl { get; set; }
    public required string OpeningHours { get; set; }
    public required string Address { get; set; }
    public decimal Latitude { get; set; }   // vĩ độ
    public decimal Longitude { get; set; }  // kinh độ
    
    public required Guid UserId { get; set; }
    public User? User { get; set; }

    public ICollection<ApplicationMenu> ApplicationMenus { get; set; } = new List<ApplicationMenu>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
