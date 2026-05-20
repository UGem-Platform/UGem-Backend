using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;

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
    
    public async Task<List<Response.SearchUserByPhoneNumberResponse>> SearchUserByPhoneNumber(string? phoneNumber, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return new List<Response.SearchUserByPhoneNumberResponse>();
        }

        var keyword = phoneNumber.Trim();
        var take = Math.Clamp(limit, 1, 20);

        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.IsActive
                        && (x.Role == "Customer" || x.Role == "Reviewer")
                        && x.Customer != null
                        && x.PhoneNumber.Contains(keyword))
            .OrderBy(x => x.PhoneNumber)
            .Select(x => new Response.SearchUserByPhoneNumberResponse
            {
                UserId = x.Id,
                CustomerId = x.Customer!.Id,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Role = x.Role,
                AvatarUrl = x.AvatarUrl
            })
            .Take(take)
            .ToListAsync();

        return users;
    }

    public async Task<List<Response.SearchUserByPhoneNumberResponse>> SearchUserByEmail(string? email, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new List<Response.SearchUserByPhoneNumberResponse>();
        }

        var keyword = email.Trim();
        var take = Math.Clamp(limit, 1, 20);

        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.IsActive
                        && (x.Role == "Customer" || x.Role == "Reviewer")
                        && x.Customer != null
                        && x.Email.Contains(keyword))
            .OrderBy(x => x.Email)
            .Select(x => new Response.SearchUserByPhoneNumberResponse
            {
                UserId = x.Id,
                CustomerId = x.Customer!.Id,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Role = x.Role,
                AvatarUrl = x.AvatarUrl
            })
            .Take(take)
            .ToListAsync();
    }

}
