using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class Customer : BaseEntity<Guid>, IAuditableEntity
{
    public int TotalCheckIns { get; set; }
    
    public string Province { get; set; } = "";
    public string District { get; set; } = "";
    public string Ward { get; set; } = "";

    public string AddressDetail { get; set; } = "";
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Reviewer? Reviewer { get; set; }
    public ICollection<ReviewerApplication> ReviewerApplications { get; set; } = new List<ReviewerApplication>(); 

    public Wishlist? Wishlist { get; set; }
    
    public ICollection<Order> Orders { get; set; } =  new List<Order>();
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
