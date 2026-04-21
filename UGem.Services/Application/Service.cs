using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.Application;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public Task<string> CreateApplicationRequest(Request.CreateApplicationRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;

        var userIdGuid = Guid.Parse(userId!);

        // var application = new Repositories.Entity.Application()
        // {
        //     UserId = userIdGuid,
        //     Type = request.Type,
        //     Status = "Pending",
        //     ReviewedAt = DateTime.UtcNow,
        // };

        throw new NotImplementedException();
    }

    public async Task AcceptApplication(Guid id, Guid staffId)
    {
        var application = await _dbContext.Applications.Include(application => application.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null) throw new Exception("Cannot found application");
        if (application.Status != "Pending") throw new Exception("The application is not pending.");
        application.Status = "Approved";
        application.ReviewedAt = DateTime.UtcNow;

        var merchant = new Merchant()
        {
            UserId = application.UserId,
            Name = application.User!.FullName,
            Email = application.User.Email,
            Phone = application.User.PhoneNumber,
            Address = "",
            LogoUrl = application.User.AvatarUrl ?? "",
            Status = "Active",
            IsActive = true,
            OpeningHours = "",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Merchants.Add(merchant);
        application.User.Role = "Merchant";
        var notification = new Repositories.Entity.Notification()
        {
            UserId = application.UserId,
            Title = "Your application has been created",
            Message = $"Congratulate, Well come {application.User.FullName}! ",
            Type = "Application",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Response.GetApplicationForStaffResponse>> GetApplications(string? status = null)
    {
        var query = _dbContext.Applications
            .Include(a => a.User)
            .Include(a => a.ApplicationMenus)
            .Where(a => status == null || a.Status == status)
            .OrderByDescending(a => a.CreatedAt);

        var selectQuery = query.Select(a => new Response.GetApplicationForStaffResponse
        {
            Id = a.Id,
            Type = a.Type,
            Status = a.Status,
            CreatedAt = a.CreatedAt,
            ReviewedAt = a.ReviewedAt,
            UpdatedAt = a.UpdatedAt,
            Applicant = new Response.ApplicantInfoResponse
            {
                UserId = a.UserId,
                FullName = a.User!.FullName,
                Email = a.User.Email,
                PhoneNumber = a.User.PhoneNumber,
                AvatarUrl = a.User.AvatarUrl,
            },

            ApplicationMenus = a.ApplicationMenus.Select(m => new Response.ApplicationMenuResponse
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                ImageUrl = m.ImageUrl,
                Category = m.Category,
            }).ToList(),
        });

        var listResult = await selectQuery.ToListAsync();
        return listResult;
    }
}