using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.AdminService;

public class Service : IService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;
    
    private readonly  AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Base.Response.PageResult<Response.StaffResponse>> GetAllStaffForAdmin(string? searchTerm, int pageSize, int pageIndex)
    {
    
        var (normalizedPageIndex, normalizedPageSize) = NormalizePagination(pageIndex, pageSize);
        var query = _dbContext.Staffs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s => s.User.FullName.Contains(searchTerm) || s.User.Email.Contains(searchTerm) ||
                                    s.User.PhoneNumber != null && s.User.PhoneNumber.Contains(searchTerm));
        }
        var totalItems = await query.CountAsync();
        var listResult = await query.OrderByDescending(x => x.CreatedAt).Skip((normalizedPageIndex - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(s => new Response.StaffResponse
            {
                Id = s.Id,
                UserId =  s.UserId,
                FullName = s.User.FullName,
                Email = s.User.Email,
                PhoneNumber = s.User.PhoneNumber,
                AvatarUrl =  s.User.AvatarUrl,
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
        var existEmail = await  _dbContext.Staffs.AnyAsync(s => s.User.Email == request.Email);
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
        var today = DateTimeOffset.UtcNow;
        var totalUsers = await _dbContext.Users.CountAsync();
        var totalMerchants = await _dbContext.Merchants.CountAsync();
        var totalOrders = await _dbContext.Orders.CountAsync();
        var totalRevenue = await _dbContext.Orders.Where(x => x.Status == "Completed").SumAsync(x => x.FinalPrice);
        var newUserToday = await _dbContext.Users.CountAsync(u => u.CreatedAt.Date == today);
        var pendingApplication = await _dbContext.Applications.CountAsync(a => a.Status == "Pending");
        var pendingReviewerApplication = await _dbContext.ReviewerApplications.CountAsync(a => a.Status == "Pending");
        return new Response.DashboardResponse
        {
            TotalUsers = totalUsers,
            TotalMerchants = totalMerchants,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            PendingApplications = pendingApplication,
            PendingReviewerApplications = pendingReviewerApplication,
            NewUsersToday =  newUserToday,

        };
        
    }

    private static (int PageIndex, int PageSize) NormalizePagination(int pageIndex, int pageSize)
    {
        var normalizedPageIndex = pageIndex <= 0 ? 1 : pageIndex;
        var normalizedPageSize = pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
        return (normalizedPageIndex, normalizedPageSize);
    }
}