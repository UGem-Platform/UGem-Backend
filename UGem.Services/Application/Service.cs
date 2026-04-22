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

    public Task<List<Response.GetApplicationForStaffResponse>> GetMyApplications()
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;

        var userIdGuid = Guid.Parse(userId!);

        var query = _dbContext.Applications
            .Include(a => a.ApplicationMenus)
            .Where(a => a.UserId == userIdGuid)
            .OrderByDescending(a => a.CreatedAt);

        if (query == null)
        {
            throw new Exception("Cannot found application");
        }

        var selectQuery = query.Select(a => new Response.GetApplicationForStaffResponse
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            Type = a.Type,
            Status = a.Status,
            CreatedAt = a.CreatedAt,
            ReviewedAt = a.ReviewedAt,
            UpdatedAt = a.UpdatedAt,

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

        var listResult = selectQuery.ToList();
        return Task.FromResult(listResult);
    }

    public async Task<string> CreateApplicationRequest(Request.ApplicationRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;

        var userIdGuid = Guid.Parse(userId!);

        var application = new Repositories.Entity.Application()
        {
            UserId = userIdGuid,
            Type = "Merchant",
            Status = "Pending",
            ReviewedAt = DateTime.UtcNow,
            Name = request.Name,
            Description = request.Description,
            Email = request.Email,
            Phone = request.Phone,
            LogoUrl = request.LogoUrl,
            Longitude = request.Longitude,
            Latitude = request.Latitude,
        };

        _dbContext.Add(application);
        await _dbContext.SaveChangesAsync();

        List<ApplicationMenu> applicationMenus = new List<ApplicationMenu>();

        foreach (var menu in request.Menu)
        {
            applicationMenus.Add(new ApplicationMenu()
            {
                ApplicationId = application.Id,
                Name = menu.Name,
                Description = menu.Description,
                Price = menu.Price,
                ImageUrl = menu.ImageUrl,
                Category = menu.Category,
            });
        }

        if (applicationMenus.Any())
        {
            _dbContext.AddRange(applicationMenus);
            await _dbContext.SaveChangesAsync();
        }

        return "Application send successfully";
    }

    public async Task<string> RejectApplication(Request.RejectApplicationRequest request)
    {
        var application = await _dbContext.Applications.FirstOrDefaultAsync(x => x.Id == request.ApplicationId);

        if (application == null)
            throw new Exception("Application not found");

        if (application.Status != "Pending")
            throw new Exception("Application already processed");

        application.Status = "Rejected";
        application.Note = request.Note;
        application.ReviewedAt = DateTime.UtcNow;

        var notification = new Notification()
        {
            UserId = application.UserId,
            Title = "Your application has been reject",
            Message = $"{application.Note}",
            Type = "Reject",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Notifications.Add(notification);

        await _dbContext.SaveChangesAsync();
        return "Reject success";
    }

    public async Task<string> EditApplicationAfterReject(Request.UpdateApplicationRequest request)
    {
        var application = await _dbContext.Applications
            .Include(x => x.ApplicationMenus)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == request.ApplicationId);

        if (application == null)
            throw new Exception("Application not found");

        if (application.Status != "Rejected")
            throw new Exception("Just edit when application reject");


        application.Type = request.Type;
        application.Note = request.Note;
        application.Status = "Pending";
        application.ReviewedAt = default;
        application.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.ApplicationMenus.RemoveRange(application.ApplicationMenus);

        foreach (var menuRequest in request.Menu)
        {
            var appMenu = new ApplicationMenu
            {
                ApplicationId = application.Id,
                Name = menuRequest.Name,
                Price = menuRequest.Price,
                Description = menuRequest.Description,
                Category = menuRequest.Category,
                ImageUrl = menuRequest.ImageUrl,
            };
            _dbContext.ApplicationMenus.Add(appMenu);
        }

        await _dbContext.SaveChangesAsync();

        return "update success";
    }

    public async Task AcceptApplication(Guid id)
    {
        var application = await _dbContext.Applications.Include(application => application.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null) throw new Exception("Cannot found application");
        if (application.User == null) throw new Exception("Application user not found");
        if (application.Status != "Pending") throw new Exception("The application is not pending.");
        application.Status = "Approved";
        application.ReviewedAt = DateTime.UtcNow;

        var merchant = new Merchant()
        {
            UserId = application.UserId,
            Name = application.Name,
            Description = application.Description,
            Email = application.Email,
            Phone = application.Phone,
            Address = "",
            LogoUrl = application.LogoUrl ?? "",
            Status = "Active",
            IsActive = true,
            OpeningHours = "",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Merchants.Add(merchant);
        application.User.Role = "Merchant";
        var notification = new Notification()
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

    public async Task<List<Response.GetApplicationForStaffResponse>> GetApplications()
    {
        var query = _dbContext.Applications
            .Include(a => a.User)
            .Include(a => a.ApplicationMenus)
            .Where(a => a.Status == "Pending")
            .OrderByDescending(a => a.CreatedAt);

        var selectQuery = query.Select(a => new Response.GetApplicationForStaffResponse
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
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