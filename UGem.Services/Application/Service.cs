using Microsoft.AspNetCore.Http;
using UGem.Repositories;

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
}