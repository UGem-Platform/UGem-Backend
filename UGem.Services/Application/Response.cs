namespace UGem.Services.Application;

public class Response
{
    public class ApplicationMenuResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
    }

    public class ApplicantInfoResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }

    public class GetApplicationForStaffResponse
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime ReviewedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        public ApplicantInfoResponse Applicant { get; set; } = null!;
        public List<ApplicationMenuResponse> ApplicationMenus { get; set; } = new();
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? RestaurantType { get; set; }
        public string? MainDishType { get; set; }
        public decimal? PriceRange { get; set; }
    }
}
