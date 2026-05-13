namespace UGem.Services.AdminService;

public class Response
{
    public class StaffResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset HiredAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
    public class DashboardResponse
    {
        public int TotalUsers { get; set; }
        public int TotalMerchants { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NewUsersToday { get; set; }
        public int PendingApplications { get; set; }
        public int PendingReviewerApplications { get; set; }
    }
}