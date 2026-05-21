using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.AdminService;

public class Service : IService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    private readonly AppDbContext _dbContext;
    private readonly UGem.Services.MonetizationService.IService _monetizationService;

    public Service(
        AppDbContext dbContext,
        UGem.Services.MonetizationService.IService monetizationService)
    {
        _dbContext = dbContext;
        _monetizationService = monetizationService;
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
        _dbContext.Notifications.Add(new Notification
        {
            UserId = user.Id,
            Title = "Staff account created",
            Message = "Your staff account has been created successfully.",
            Type = "Staff",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
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
        _dbContext.Notifications.Add(new Notification
        {
            UserId = staff.UserId,
            Title = "Staff account deactivated",
            Message = "Your staff account has been deactivated.",
            Type = "Staff",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Response.DashboardResponse> GetDashboard()
    {
        await _monetizationService.ProcessCompletedOrdersMissingMonetization();

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
        var completedOrderStats = await _dbContext.Orders
            .Where(o => o.Status == "Completed")
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalRevenue = g.Sum(o => o.FinalPrice),
                TotalPlatformFee = g.Sum(o => o.PlatformFee),
                TotalReviewerFee = g.Sum(o => o.ReviewerFee),
                TotalCompletedOrders = g.Count()
            })
            .FirstOrDefaultAsync();

        var totalRevenue = completedOrderStats?.TotalRevenue ?? 0;
        var totalPlatformFee = completedOrderStats?.TotalPlatformFee ?? 0;
        var totalReviewerFee = completedOrderStats?.TotalReviewerFee ?? 0;
        var totalCompletedOrders = completedOrderStats?.TotalCompletedOrders ?? 0;
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

    public async Task<List<Response.MerchantRevenueResponse>> GetMerchantRevenues(string? searchTerm, int pageIndex, int pageSize)
{
    await _monetizationService.ProcessCompletedOrdersMissingMonetization();

    var (normalizedPageIndex, normalizedPageSize) =
        NormalizePagination(pageIndex, pageSize);

    var now = DateTimeOffset.UtcNow;

    var currentPeriodStart = new DateTimeOffset(
        now.Year,
        now.Month, 1, 0, 0, 0, TimeSpan.Zero);

    var previousPeriodStart = currentPeriodStart.AddMonths(-1);

    var merchantQuery = _dbContext.Merchants
        .AsNoTracking();

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        merchantQuery = merchantQuery.Where(m =>
            m.Name.Contains(searchTerm));
    }

    var merchants = await merchantQuery
        .OrderBy(m => m.Name)
        .Skip((normalizedPageIndex - 1) * normalizedPageSize)
        .Take(normalizedPageSize)
        .ToListAsync();

    var merchantIds = merchants.Select(m => m.Id).ToList();

    var orderStats = await _dbContext.Orders
        .Where(o => o.Status == "Completed"
                    && o.OrderDetails.Any(od => od.Food != null && merchantIds.Contains(od.Food.MerchantId)))
        .GroupBy(o => o.OrderDetails
            .Select(od => od.Food.MerchantId)
            .FirstOrDefault())
        .Select(g => new
        {
            MerchantId = g.Key,
            TotalOrders = g.Count(),
            TotalRevenue = g.Sum(o => o.FinalPrice),
            PlatformFee = g.Sum(o => o.PlatformFee),
            ReviewerFee = g.Sum(o => o.ReviewerFee),
            LastOrderAt = g.Max(o => o.CreatedAt),

            CurrentRevenue = g
                .Where(o => o.CreatedAt >= currentPeriodStart)
                .Sum(o => o.FinalPrice),

            PreviousRevenue = g
                .Where(o => o.CreatedAt >= previousPeriodStart
                            && o.CreatedAt < currentPeriodStart)
                .Sum(o => o.FinalPrice)
        })
        .ToDictionaryAsync(x => x.MerchantId);

    return merchants
        .Select(merchant =>
        {
            var stats = orderStats.GetValueOrDefault(merchant.Id);

            var totalRevenue = stats?.TotalRevenue ?? 0;
            var platformFee = stats?.PlatformFee ?? 0;
            var reviewerFee = stats?.ReviewerFee ?? 0;
            var totalOrders = stats?.TotalOrders ?? 0;
            var currentRevenue = stats?.CurrentRevenue ?? 0;
            var previousRevenue = stats?.PreviousRevenue ?? 0;

            var revenueGrowth = previousRevenue > 0
                ? Math.Round((currentRevenue - previousRevenue) / previousRevenue * 100, 2) : 0;

            return new Response.MerchantRevenueResponse
            {
                MerchantId = merchant.Id,
                MerchantName = merchant.Name,
                LogoUrl = merchant.LogoUrl,
                CompletedOrders = totalOrders,
                TotalRevenue = totalRevenue,
                PlatformFee = Math.Round(platformFee, 2),
                ReviewerFee = Math.Round(reviewerFee, 2),
                MerchantReceive = Math.Round(totalRevenue - platformFee - reviewerFee, 2),
                AverageOrderValue = totalOrders > 0 ? Math.Round(totalRevenue / totalOrders, 2) : 0,
                LastOrderAt = stats?.LastOrderAt,
                RevenueGrowth = revenueGrowth
            };
        })
        .OrderByDescending(m => m.TotalRevenue)
        .ToList();
}

    public async Task<Response.MerchantDetailResponse> GetMerchantDetail(Guid merchantId, string periodType)
    {
        var merchant = await _dbContext.Merchants
                           .AsNoTracking()
                           .FirstOrDefaultAsync(m => m.Id == merchantId)
                       ?? throw new KeyNotFoundException("Merchant not found");
        
        var stats = await _dbContext.Orders
            .Where(o => o.OrderDetails.Any(od => od.Food.MerchantId == merchantId))
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalRevenue = g.Where(o => o.Status == "Completed").Sum(o => o.FinalPrice),
                PlatformFee = g.Where(o => o.Status == "Completed").Sum(o => o.PlatformFee),
                ReviewerFee = g.Where(o => o.Status == "Completed").Sum(o => o.ReviewerFee),
                CompletedOrders = g.Count(o => o.Status == "Completed"),
                PendingOrders = g.Count(o => o.Status == "Pending"),
                AcceptedOrders = g.Count(o => o.Status == "Accepted"),
                RejectedOrders = g.Count(o => o.Status == "Rejected"),
                TotalOrders = g.Count(),
                LastOrderAt = g.Where(o => o.Status == "Completed").Max(o => (DateTimeOffset?)o.CreatedAt),
                TotalUniqueCustomers = g.Where(o => o.Status == "Completed")
                    .Select(o => o.CustomerId).Distinct().Count()
            })
            .FirstOrDefaultAsync();

        var totalRevenue = stats?.TotalRevenue ?? 0;
        var platformFee = stats?.PlatformFee ?? 0;
        var reviewerFee = stats?.ReviewerFee ?? 0;
        var completedOrders = stats?.CompletedOrders ?? 0;
        var totalOrders = stats?.TotalOrders ?? 0;
        var rejectedOrders = stats?.RejectedOrders ?? 0;
        var completedOrdersQuery = _dbContext.Orders
            .Where(o => o.Status == "Completed"
                        && o.OrderDetails.Any(od => od.Food.MerchantId == merchantId));

        List<Response.RevenueByPeriod> revenueChart;

        if (periodType == "Day")
        {
            revenueChart = await completedOrdersQuery
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month, o.CreatedAt.Day })
                .Select(g => new Response.RevenueByPeriod
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}-{g.Key.Day:D2}",
                    PeriodType = "Day",
                    Revenue = g.Sum(o => o.FinalPrice),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToListAsync();
        }
        else if (periodType == "Week")
        {
            var weekStart = DateTimeOffset.UtcNow.AddDays(-84);
            var orders = await completedOrdersQuery
                .Where(o => o.CreatedAt >= weekStart)
                .Select(o => new
                {
                    o.CreatedAt,
                    o.FinalPrice
                })
                .ToListAsync();

            revenueChart = orders
                .GroupBy(o => $"{o.CreatedAt.Year}-W{GetIso8601WeekOfYear(o.CreatedAt.DateTime)}")
                .Select(g => new Response.RevenueByPeriod
                {
                    Period = g.Key,
                    PeriodType = "Week",
                    Revenue = g.Sum(o => o.FinalPrice),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToList();
        }
        else
        {
            revenueChart = await completedOrdersQuery
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new Response.RevenueByPeriod
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    PeriodType = "Month",
                    Revenue = g.Sum(o => o.FinalPrice),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToListAsync();
        }

        // Top foods - query riêng
        var topFoods = await _dbContext.OrderDetails
            .Where(od => od.Food.MerchantId == merchantId
                         && od.Order.Status == "Completed")
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
            .ToListAsync();

        return new Response.MerchantDetailResponse
        {
            MerchantId = merchant.Id,
            MerchantName = merchant.Name,
            LogoUrl = merchant.LogoUrl,
            TotalRevenue = totalRevenue,
            PlatformFee = Math.Round(platformFee, 2),
            ReviewerFee = Math.Round(reviewerFee, 2),
            MerchantReceive = Math.Round(totalRevenue - platformFee - reviewerFee, 2),
            AverageOrderValue = completedOrders > 0 ? Math.Round(totalRevenue / completedOrders, 2) : 0,
            PendingOrders = stats?.PendingOrders ?? 0,
            AcceptedOrders = stats?.AcceptedOrders ?? 0,
            RejectedOrders = rejectedOrders,
            CompletedOrders = completedOrders,
            CancellationRate = totalOrders > 0
                ? Math.Round((decimal)rejectedOrders / totalOrders * 100, 2)
                : 0,
            TotalUniqueCustomers = stats?.TotalUniqueCustomers ?? 0,
            LastOrderAt = stats?.LastOrderAt,
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