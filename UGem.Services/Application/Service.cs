using Microsoft.AspNetCore.Http;
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
    
    public async Task<string> CreateApplicationRequest(Request.CreateApplicationRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        
        var userIdGuid = Guid.Parse(userId!);
        
        var application = new Repositories.Entity.Application()
        {
            UserId = userIdGuid,
            Type = "Merchant",
            Status = "Pending",
            ReviewedAt = DateTime.UtcNow,
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
}