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
        var userId = _httpContext.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "UserId")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("UserId claim is missing.");
        }

        var userIdGuid = Guid.Parse(userId);
        var query = _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userIdGuid)
            .OrderByDescending(x => x.CreatedAt);

        var notificationResponse = query.Select(x => new Response.NotificationResponse()
        {
            Id = x.Id,
            Title = x.Title,
            Message = x.Message,
            Type = x.Type,
            IsRead = x.IsRead,
            CreatedAt = x.CreatedAt
        });
        
        var result = await notificationResponse.ToListAsync();
        
        return result;
    }

    public async Task MarkAsRead(Guid notificationId)
    {
        var userId = _httpContext.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "UserId")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("UserId claim is missing.");
        }

        var userIdGuid = Guid.Parse(userId);

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userIdGuid);

        if (notification == null)
        {
            throw new KeyNotFoundException("Notification not found");
        }

        notification.IsRead = true;
        notification.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
    public async Task MarkAllAsRead()
    {
        var userId = _httpContext.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "UserId")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("UserId claim is missing.");
        }

        var userIdGuid = Guid.Parse(userId);

        var unreadNotifications = await _dbContext.Notifications
            .Where(x => x.UserId == userIdGuid && !x.IsRead)
            .ToListAsync();

        if (unreadNotifications.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync();
    }
}