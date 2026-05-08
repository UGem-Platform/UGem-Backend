using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.Application;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly MediaService.IService _mediaService;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext, MediaService.IService mediaService)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _mediaService = mediaService;
    }

    public async Task<List<Response.GetApplicationForStaffResponse>> GetMyApplications()
    {
        var userIdGuid = GetRequiredGuidClaim("UserId");

        return await _dbContext.Applications
            .AsNoTracking()
            .Where(a => a.UserId == userIdGuid)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new Response.GetApplicationForStaffResponse
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                RestaurantType = a.RestaurantType,
                MainDishType = a.MainDishType,
                PriceRange = a.PriceRange,
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
            })
            .ToListAsync();
    }

    public async Task<string> CreateApplicationRequest(Request.ApplicationRequest request)
    {
        var userIdGuid = GetRequiredGuidClaim("UserId");

        var merchantExistsForUser = await _dbContext.Merchants
            .AnyAsync(m => m.UserId == userIdGuid);

        if (merchantExistsForUser)
        {
            throw new InvalidOperationException("This user already has a merchant profile.");
        }

        var applicationExistsForUser = await _dbContext.Applications
            .AnyAsync(m => m.UserId == userIdGuid);

        if (applicationExistsForUser)
        {
            throw new InvalidOperationException("This user already has an application.");
        }

        var applicationId = Guid.NewGuid();

        var application = new Repositories.Entity.Application
        {
            Id = applicationId,
            UserId = userIdGuid,
            Type = "Merchant",
            Status = "Pending",
            ReviewedAt = DateTime.UtcNow,
            Name = request.Name,
            Description = request.Description,
            RestaurantType = request.RestaurantType,
            MainDishType = request.MainDishType,
            PriceRange = request.PriceRange,
            Email = request.Email,
            Phone = request.Phone,
            LogoUrl = request.LogoUrl,
            OpeningHours = request.OpeningHours,
            Address = request.Address,
            Longitude = request.Longitude,
            Latitude = request.Latitude,
            ApplicationMenus = request.Menu.Select(menu => new ApplicationMenu
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                Name = menu.Name,
                Description = menu.Description,
                Price = menu.Price,
                Category = menu.Category,
                ImageUrl = menu.ImageUrl
            }).ToList()
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();

        return "Application send successfully";
    }

    public async Task<string> RejectApplication(Request.RejectApplicationRequest request)
    {
        var application = await _dbContext.Applications.FirstOrDefaultAsync(x => x.Id == request.ApplicationId);

        if (application == null)
        {
            throw new KeyNotFoundException("Application not found");
        }

        if (application.Status != "Pending")
        {
            throw new InvalidOperationException("Application already processed");
        }

        application.Status = "Rejected";
        application.Note = request.Note;
        application.ReviewedAt = DateTime.UtcNow;

        _dbContext.Notifications.Add(new Notification
        {
            UserId = application.UserId,
            Title = "Your application has been reject",
            Message = application.Note ?? string.Empty,
            Type = "Reject",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await _dbContext.SaveChangesAsync();
        return "Reject success";
    }

    public async Task<string> EditApplicationAfterReject(Request.UpdateApplicationRequest request)
    {
        var userIdGuid = GetRequiredGuidClaim("UserId");

        var application = await _dbContext.Applications
            .Include(x => x.ApplicationMenus)
            .FirstOrDefaultAsync(x => x.Id == request.ApplicationId);

        if (application == null)
        {
            throw new KeyNotFoundException("Application not found");
        }

        if (application.UserId != userIdGuid)
        {
            throw new UnauthorizedAccessException("Cannot edit another user's application.");
        }

        if (application.Status != "Rejected")
        {
            throw new InvalidOperationException("Just edit when application reject");
        }

        application.Type = request.Type;
        application.Name = request.Name;
        application.Description = request.Description;
        application.RestaurantType = request.RestaurantType;
        application.MainDishType = request.MainDishType;
        application.PriceRange = request.PriceRange;
        application.Email = request.Email;
        application.Phone = request.Phone;
        application.LogoUrl = request.LogoUrl;
        application.OpeningHours = request.OpeningHours;
        application.Address = request.Address;
        application.Latitude = request.Latitude;
        application.Longitude = request.Longitude;
        application.Note = request.Note;
        application.Status = "Pending";
        application.ReviewedAt = default;
        application.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.ApplicationMenus.RemoveRange(application.ApplicationMenus);
        foreach (var menuRequest in request.Menu)
        {
            _dbContext.ApplicationMenus.Add(new ApplicationMenu
            {
                ApplicationId = application.Id,
                Name = menuRequest.Name,
                Price = menuRequest.Price,
                Description = menuRequest.Description,
                Category = menuRequest.Category,
                ImageUrl = menuRequest.ImageUrl,
            });
        }

        await _dbContext.SaveChangesAsync();
        return "update success";
    }

    public async Task AcceptApplication(Guid id)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var application = await _dbContext.Applications
            .Include(x => x.User)
            .Include(x => x.ApplicationMenus)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
        {
            throw new KeyNotFoundException("Application not found.");
        }

        if (application.User == null)
        {
            throw new InvalidOperationException("Application user not found.");
        }

        if (application.Status != "Pending")
        {
            throw new InvalidOperationException("The application is not pending.");
        }

        if (application.Latitude is < -90 or > 90 || application.Longitude is < -180 or > 180)
        {
            throw new InvalidOperationException("Application coordinates are invalid.");
        }

        var existingMerchant = await _dbContext.Merchants
            .FirstOrDefaultAsync(m => m.UserId == application.UserId);

        var merchantEmailExists = await _dbContext.Merchants
            .AnyAsync(m => m.Email == application.Email && m.UserId != application.UserId);

        if (merchantEmailExists)
        {
            throw new InvalidOperationException("This application email is already used by another merchant.");
        }

        application.Status = "Approved";
        application.ReviewedAt = DateTime.UtcNow;
        application.User.Role = "Merchant";

        var location = new Point((double)application.Longitude, (double)application.Latitude) { SRID = 4326 };

        if (existingMerchant != null)
        {
            existingMerchant.Name = application.Name;
            existingMerchant.Description = application.Description;
            existingMerchant.RestaurantType = application.RestaurantType;
            existingMerchant.MainDishType = application.MainDishType;
            existingMerchant.PriceRange = application.PriceRange;
            existingMerchant.Email = application.Email;
            existingMerchant.Phone = application.Phone;
            existingMerchant.Address = application.Address;
            existingMerchant.LogoUrl = application.LogoUrl ?? string.Empty;
            existingMerchant.Status = "Active";
            existingMerchant.IsActive = true;
            existingMerchant.OpeningHours = application.OpeningHours;
            existingMerchant.Latitude = (double)application.Latitude;
            existingMerchant.Longitude = (double)application.Longitude;
            existingMerchant.Location = location;
            existingMerchant.UpdatedAt = DateTimeOffset.UtcNow;

            SyncMerchantMenu(existingMerchant.Id, application.ApplicationMenus);
        }
        else
        {
            var merchantId = Guid.NewGuid();
            var merchant = new Merchant
            {
                Id = merchantId,
                UserId = application.UserId,
                Name = application.Name,
                Description = application.Description,
                RestaurantType = application.RestaurantType,
                MainDishType = application.MainDishType,
                PriceRange = application.PriceRange,
                Email = application.Email,
                Phone = application.Phone,
                Address = application.Address,
                LogoUrl = application.LogoUrl ?? string.Empty,
                Status = "Active",
                IsActive = true,
                OpeningHours = application.OpeningHours,
                Latitude = (double)application.Latitude,
                Longitude = (double)application.Longitude,
                Location = location,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            _dbContext.Merchants.Add(merchant);
            SyncMerchantMenu(merchantId, application.ApplicationMenus);
        }

        _dbContext.Notifications.Add(new Notification
        {
            UserId = application.UserId,
            Title = "Your application has been approved",
            Message = $"Congratulate, Well come {application.User.FullName}! ",
            Type = "Application",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<List<Response.GetApplicationForStaffResponse>> GetApplications()
    {
        return await _dbContext.Applications
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new Response.GetApplicationForStaffResponse
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                RestaurantType = a.RestaurantType,
                MainDishType = a.MainDishType,
                PriceRange = a.PriceRange,
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
            })
            .ToListAsync();
    }

    private void SyncMerchantMenu(Guid merchantId, IEnumerable<ApplicationMenu> applicationMenus)
    {
        var existingFoods = _dbContext.Foods.Where(f => f.MerchantId == merchantId);
        _dbContext.Foods.RemoveRange(existingFoods);

        var foods = applicationMenus.Select(menu => new Food
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Name = menu.Name,
            Description = menu.Description,
            Price = menu.Price,
            ImageUrl = menu.ImageUrl,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        _dbContext.Foods.AddRange(foods);
    }

    private Guid GetRequiredGuidClaim(string claimType)
    {
        var rawValue = _httpContext.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw new UnauthorizedAccessException($"{claimType} claim is missing");
        }

        return Guid.Parse(rawValue);
    }
}
