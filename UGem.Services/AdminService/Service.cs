using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.AdminService;

public class Service : IService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Base.Response.PageResult<Response.StaffResponse>> GetAllStaffForAdmin(string? searchTerm,
        int pageSize, int pageIndex)
    {
        var (normalizedPageIndex, normalizedPageSize) = NormalizePagination(pageIndex, pageSize);
        var query = _dbContext.Staffs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s => s.User.FullName.Contains(searchTerm) || s.User.Email.Contains(searchTerm) ||
                                     s.User.PhoneNumber != null && s.User.PhoneNumber.Contains(searchTerm));
        }

        var totalItems = await query.CountAsync();
        var listResult = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((normalizedPageIndex - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(s => new Response.StaffResponse
            {
                Id = s.Id,
                UserId = s.UserId,
                FullName = s.User.FullName,
                Email = s.User.Email,
                PhoneNumber = s.User.PhoneNumber,
                AvatarUrl = s.User.AvatarUrl,
                CreatedAt = s.CreatedAt,
                IsActive = s.User.IsActive,
                HiredAt = s.HiredAt,
            }).ToListAsync();
        return new Base.Response.PageResult<Response.StaffResponse>
        {
            Items = listResult,
            TotalItems = totalItems,
            PageIndex = normalizedPageIndex,
            PageSize = normalizedPageSize
        };
    }

    public async Task CreateStaff(Request.CreateStaffRequest request)
    {
        if (!System.Net.Mail.MailAddress.TryCreate(request.Email, out _))
            throw new ArgumentException("Invalid email format");
        var existEmail = await _dbContext.Staffs.AnyAsync(s => s.User.Email == request.Email);
        if (existEmail)
        {
            throw new ArgumentException("Email already exists");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            Role = "Staff",
            IsActive = true,
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.Staffs.Add(new Staff
        {
            UserId = user.Id,
            HiredAt = DateTimeOffset.UtcNow,
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteStaff(Guid staffId)
    {
        var staff = _dbContext.Staffs.Include(s => s.User).FirstOrDefault(s => s.Id == staffId);
        if (staff == null)
        {
            throw new ArgumentException("Staff not found");
        }

        staff.IsDeleted = true;
        staff.User.IsActive = false;
        staff.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Response.DashboardResponse> GetDashboard()
    {
        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var tomorrowStart = todayStart.AddDays(1);

        var totalUsers = await _dbContext.Users.CountAsync();
        var totalMerchants = await _dbContext.Merchants.CountAsync();
        var newUsersToday = await _dbContext.Users
            .CountAsync(u => u.CreatedAt >= todayStart && u.CreatedAt < tomorrowStart);
        var pendingApplications = await _dbContext.Applications
            .CountAsync(a => a.Status == "Pending");
        var pendingReviewerApplications = await _dbContext.ReviewerApplications
            .CountAsync(a => a.Status == "Pending");
        var completedOrders = await _dbContext.Orders
            .Where(o => o.Status == "Completed")
            .ToListAsync();

        var totalRevenue = completedOrders.Sum(o => o.FinalPrice);
        var totalPlatformFee = completedOrders.Sum(o => o.PlatformFee);
        var totalReviewerFee = completedOrders.Sum(o => o.ReviewerFee);
        var totalCompletedOrders = completedOrders.Count;
        var averageOrderValue = totalCompletedOrders > 0
            ? Math.Round(totalRevenue / totalCompletedOrders, 2)
            : 0;

        return new Response.DashboardResponse
        {
            TotalUsers = totalUsers,
            TotalMerchants = totalMerchants,
            NewUsersToday = newUsersToday,
            PendingApplications = pendingApplications,
            PendingReviewerApplications = pendingReviewerApplications,
            TotalRevenue = totalRevenue,
            TotalPlatformFee = totalPlatformFee,
            TotalReviewerFee = totalReviewerFee,
            TotalCompletedOrders = totalCompletedOrders,
            AverageOrderValue = averageOrderValue
        };
    }

    public async Task<List<Response.MerchantRevenueResponse>> GetMerchantRevenues()
    {
        var merchants = await _dbContext.Merchants
            .AsNoTracking()
            .ToListAsync();

        var now = DateTimeOffset.UtcNow;
        var currentPeriodStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var previousPeriodStart = currentPeriodStart.AddMonths(-1);
        var previousPeriodEnd = currentPeriodStart;

        var result = new List<Response.MerchantRevenueResponse>();

        foreach (var merchant in merchants)
        {
            var completedOrders = await _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.Status == "Completed"
                            && o.OrderDetails.Any(od => od.Food.MerchantId == merchant.Id))
                .ToListAsync();

            var totalRevenue = completedOrders.Sum(o => o.FinalPrice);
            var platformFee = completedOrders.Sum(o => o.PlatformFee);
            var reviewerFee = completedOrders.Sum(o => o.ReviewerFee);
            var merchantReceive = totalRevenue - platformFee - reviewerFee;
            var completedCount = completedOrders.Count;
            var averageOrderValue = completedCount > 0
                ? Math.Round(totalRevenue / completedCount, 2)
                : 0;
            var lastOrderAt = completedOrders
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault()?.CreatedAt;
            var currentRevenue = completedOrders
                .Where(o => o.CreatedAt >= currentPeriodStart)
                .Sum(o => o.FinalPrice);

            var previousRevenue = completedOrders
                .Where(o => o.CreatedAt >= previousPeriodStart
                            && o.CreatedAt < previousPeriodEnd)
                .Sum(o => o.FinalPrice);
            var revenueGrowth = previousRevenue > 0
                ? Math.Round((currentRevenue - previousRevenue) / previousRevenue * 100, 2)
                : 0;

            result.Add(new Response.MerchantRevenueResponse
            {
                MerchantId = merchant.Id,
                MerchantName = merchant.Name,
                LogoUrl = merchant.LogoUrl,
                CompletedOrders = completedCount,
                TotalRevenue = totalRevenue,
                PlatformFee = Math.Round(platformFee, 2),
                ReviewerFee = Math.Round(reviewerFee, 2),
                MerchantReceive = Math.Round(merchantReceive, 2),
                AverageOrderValue = averageOrderValue,
                LastOrderAt = lastOrderAt,
                RevenueGrowth = revenueGrowth
            });
        }
        return result.OrderByDescending(m => m.TotalRevenue).ToList();
    }

    public async Task<Response.MerchantDetailResponse> GetMerchantDetail(
        Guid merchantId, string periodType)
    {
        var merchant = await _dbContext.Merchants
                           .AsNoTracking()
                           .FirstOrDefaultAsync(m => m.Id == merchantId)
                       ?? throw new KeyNotFoundException("Merchant not found");
        var allOrders = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Food)
            .Where(o => o.OrderDetails.Any(od => od.Food.MerchantId == merchantId))
            .ToListAsync();

        var completedOrders = allOrders.Where(o => o.Status == "Completed").ToList();

        var totalRevenue = completedOrders.Sum(o => o.FinalPrice);
        var platformFee = completedOrders.Sum(o => o.PlatformFee);
        var reviewerFee = completedOrders.Sum(o => o.ReviewerFee);
        var merchantReceive = totalRevenue - platformFee - reviewerFee;
        var completedCount = completedOrders.Count;
        var averageOrderValue = completedCount > 0
            ? Math.Round(totalRevenue / completedCount, 2)
            : 0;
        var pendingOrders = allOrders.Count(o => o.Status == "Pending");
        var acceptedOrders = allOrders.Count(o => o.Status == "Accepted");
        var rejectedOrders = allOrders.Count(o => o.Status == "Rejected");
        var cancellationRate = allOrders.Count > 0
            ? Math.Round((decimal)rejectedOrders / allOrders.Count * 100, 2)
            : 0;
        var totalUniqueCustomers = completedOrders
            .Select(o => o.CustomerId)
            .Distinct()
            .Count();
        var lastOrderAt = completedOrders
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefault()?.CreatedAt;
        var revenueChart = periodType switch
        {
            "Day" => completedOrders
                .GroupBy(o => o.CreatedAt.ToString("yyyy-MM-dd"))
                .Select(g => new Response.RevenueByPeriod
                {
                    Period = g.Key,
                    PeriodType = "Day",
                    Revenue = g.Sum(o => o.FinalPrice),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToList(),

            "Week" => completedOrders
                .GroupBy(o => $"{o.CreatedAt.Year}-W{GetIso8601WeekOfYear(o.CreatedAt.DateTime)}")
                .Select(g => new Response.RevenueByPeriod
                {
                    Period = g.Key,
                    PeriodType = "Week",
                    Revenue = g.Sum(o => o.FinalPrice),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToList(),

            _ => completedOrders
                .GroupBy(o => o.CreatedAt.ToString("yyyy-MM"))
                .Select(g => new Response.RevenueByPeriod
                {
                    Period = g.Key,
                    PeriodType = "Month",
                    Revenue = g.Sum(o => o.FinalPrice),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToList()
        };
        
        var topFoods = completedOrders
            .SelectMany(o => o.OrderDetails)
            .Where(od => od.Food.MerchantId == merchantId)
            .GroupBy(od => new { od.FoodId, od.Food.Name })
            .Select(g => new Response.TopFood
            {
                FoodId = g.Key.FoodId,
                FoodName = g.Key.Name,
                TotalSold = g.Sum(od => od.Quantity),
                TotalRevenue = g.Sum(od => od.Quantity * od.UnitPrice)
            })
            .OrderByDescending(f => f.TotalSold)
            .Take(10)
            .ToList();

        return new Response.MerchantDetailResponse
        {
            MerchantId = merchant.Id,
            MerchantName = merchant.Name,
            LogoUrl = merchant.LogoUrl,
            TotalRevenue = totalRevenue,
            PlatformFee = Math.Round(platformFee, 2),
            ReviewerFee = Math.Round(reviewerFee, 2),
            MerchantReceive = Math.Round(merchantReceive, 2),
            AverageOrderValue = averageOrderValue,
            PendingOrders = pendingOrders,
            AcceptedOrders = acceptedOrders,
            RejectedOrders = rejectedOrders,
            CompletedOrders = completedCount,
            CancellationRate = cancellationRate,
            TotalUniqueCustomers = totalUniqueCustomers,
            LastOrderAt = lastOrderAt,
            RevenueChart = revenueChart,
            TopFoods = topFoods
        };
    }
    private static int GetIso8601WeekOfYear(DateTime date)
    {
        var day = System.Globalization.CultureInfo.InvariantCulture
            .Calendar.GetDayOfWeek(date);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            date = date.AddDays(3);
        return System.Globalization.CultureInfo.InvariantCulture
            .Calendar.GetWeekOfYear(date,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
    }

    private static (int PageIndex, int PageSize) NormalizePagination(int pageIndex, int pageSize)
    {
        var normalizedPageIndex = pageIndex <= 0 ? 1 : pageIndex;
        var normalizedPageSize = pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
        return (normalizedPageIndex, normalizedPageSize);
    }
}