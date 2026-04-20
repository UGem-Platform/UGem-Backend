using UGem.Repositories.Abtraction;

namespace UGem.Repositories.Entity;

public class User : BaseEntity<Guid>, IAuditableEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string PhoneNumber { get; set; }
    public required string FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public string Role { get; set; } = "user";
    
    public Admin? Admin { get; set; }
    public Staff? Staff { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}