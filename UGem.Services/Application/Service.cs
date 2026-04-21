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
    
    public Task<string> CreateApplicationRequest(Request.ApplicationRequest request)
    {
        // var application = new Repositories.Entity.Application()
        // {
        //     UserId = userIdGuid,
        //     Type = request.Type,
        //     Status = "Pending",
        //     ReviewedAt = DateTime.UtcNow,
        // };
        
        throw new NotImplementedException();
    }
    
    public async Task<string> RejectApplication(Request.RejectApplicationRequest request) 
    { 
        var application = await _dbContext.Applications.FirstOrDefaultAsync(
            x => x.Id == request.ApplicationId);
        
        if (application == null) 
            throw new Exception("Application not found"); 
        
        if (application.Status != "Pending") 
            throw new Exception("Application already processed"); 
        
        application.Status = "Rejected"; 
        application.Note = request.Note;
        application.ReviewedAt = DateTime.Now; 
        
        await _dbContext.SaveChangesAsync(); 
        return "Reject success"; 
    }

    public async Task<string> EditApplicationAfterReject(Request.UpdateApplicationRequest request)
    {
        var app = await _dbContext.Applications
            .Include(x => x.ApplicationMenus)
            .FirstOrDefaultAsync(x => x.Id == request.ApplicationId);

        if (app == null)
            throw new Exception("Application not found"); 
        
        if (app.Status != "Rejected")
            throw new Exception("Just edit when application reject");
        
        
        app.Type = request.Type;
        app.Note = request.Note;
        app.Status = "Pending";
        app.ReviewedAt = default;
        app.UpdatedAt = DateTimeOffset.UtcNow;
        
        if (request.Menu != null)
        {
            _dbContext.ApplicationMenus.RemoveRange(app.ApplicationMenus);
            
            app.ApplicationMenus = request.Menu.Select(x => new ApplicationMenu()
            {
                Id = Guid.NewGuid(),
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                ImageUrl = x.ImageUrl,
                Category = x.Category,
                ApplicationId = app.Id,
                CreatedAt = DateTimeOffset.UtcNow
            }).ToList();
        }

        await _dbContext.SaveChangesAsync();

        return "update success";
    }
}