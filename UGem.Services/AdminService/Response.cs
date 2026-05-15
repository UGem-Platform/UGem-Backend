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
    public int NewUsersToday { get; set; }
    public int PendingApplications { get; set; }
    public int PendingReviewerApplications { get; set; }
    
    public decimal TotalRevenue { get; set; }
    public decimal TotalPlatformFee { get; set; }
    public decimal TotalReviewerFee { get; set; }
    public int TotalCompletedOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class MerchantRevenueResponse
{
    public Guid MerchantId { get; set; }
    public required string MerchantName { get; set; }
    public string? LogoUrl { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal ReviewerFee { get; set; }
    public decimal MerchantReceive { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTimeOffset? LastOrderAt { get; set; }
    public decimal RevenueGrowth { get; set; }
}
public class MerchantDetailResponse
{
    public Guid MerchantId { get; set; }
    public required string MerchantName { get; set; }
    public string? LogoUrl { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal ReviewerFee { get; set; }
    public decimal MerchantReceive { get; set; }
    public decimal AverageOrderValue { get; set; } 
    public int PendingOrders { get; set; }
    public int AcceptedOrders { get; set; }
    public int RejectedOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal CancellationRate { get; set; }
    public int TotalUniqueCustomers { get; set; }
    public DateTimeOffset? LastOrderAt { get; set; }
    public List<RevenueByPeriod> RevenueChart { get; set; } = new();
    public List<TopFood> TopFoods { get; set; } = new();
}

public class RevenueByPeriod
{
    
    public string Period { get; set; } = null!;
    public string PeriodType { get; set; } = null!;

    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopFood
{
    public Guid FoodId { get; set; }
    public required string FoodName { get; set; }
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

}