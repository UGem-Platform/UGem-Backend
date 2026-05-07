using NetTopologySuite.Geometries;
using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Merchant: BaseEntity<Guid>, IAuditableEntity
{
    public  required string Name { get; set; }
    public required string Description { get; set; }
    public string? RestaurantType { get; set; }
    public string? MainDishType { get; set; }
    public string? PriceRange { get; set; }
    public required  string Email { get; set; }
    public required  string Phone { get; set; }
    public required  string Address { get; set; }
    public bool IsActive { get; set; }
    public required string LogoUrl { get; set; }
    public required string Status {get; set;}
    public double UnderratedScore { get; set; }
    public decimal Rating { get; set; }
    public decimal PlatformFeePercent { get; set; }
    public required string OpeningHours { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public required Point Location { get; set; }
    
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public ICollection<Food> Foods { get; set; } = new List<Food>();
    
    public ICollection<AffiliateLink> AffiliateLinks { get; set; } = new List<AffiliateLink>();
    
    public ICollection<WishlistDetail> WishlistDetails { get; set; } = new List<WishlistDetail>();
    
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
