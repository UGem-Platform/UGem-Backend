using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Merchant: BaseEntity<Guid>, IAuditableEntity
{
    public  required string Name { get; set; }
    public required  string Email { get; set; }
    public required  string Phone { get; set; }
    public required  string Address { get; set; }
    public bool IsActive { get; set; }
    public required string LogoUrl { get; set; }
    public required string Status {get; set;}
    public decimal UnderratedScore { get; set; }
    public double rating { get; set; }
    public decimal PlatFormFeePercent { get; set; }
    public required string OpeningHours { get; set; }
    public decimal Latitude { get; set; }   // vĩ độ
    public decimal Longitude { get; set; }  // kinh độ
    
    
    public ICollection<Food> Foods { get; set; } = new List<Food>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}