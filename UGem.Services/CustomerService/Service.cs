using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;
using UGem.Services.MailService;

namespace UGem.Services.CustomerService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
   
    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
     
    }
    
    public async Task<List<Response.SearchUserByEmailResponse>> SearchUserByEmail(string? email, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new List<Response.SearchUserByEmailResponse>();
        }

        var keyword = email.Trim().ToLower();
        var take = Math.Clamp(limit, 1, 20);

        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.IsActive
                        && x.Role == "Customer"
                        && x.Customer != null
                        && x.Email.ToLower().Contains(keyword))
            .OrderBy(x => x.Email)
            .Select(x => new Response.SearchUserByEmailResponse
            {
                UserId = x.Id,
                CustomerId = x.Customer!.Id,
                FullName = x.FullName,
                Email = x.Email,
                Role = x.Role,
                AvatarUrl = x.AvatarUrl
            })
            .Take(take)
            .ToListAsync();

        return users;
    }

}