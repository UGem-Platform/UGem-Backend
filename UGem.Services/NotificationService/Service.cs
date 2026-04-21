using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;

namespace UGem.Services.NotificationService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }
    
    public async Task<List<Response.NotificationResponse>> GetNotificationRequests()
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        
        var userIdGuid = Guid.Parse(userId!);
        
        var query = _dbContext.Notifications.Where(x => x.UserId == userIdGuid);

        var notificationResponse = query.Select(x => new Response.NotificationResponse()
        {
            Title = x.Title,
            Message = x.Message,
            IsRead = x.IsRead
        });
        
        var result = await notificationResponse.ToListAsync();
        
        return result;
    }
}