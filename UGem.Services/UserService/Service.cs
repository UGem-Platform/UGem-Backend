using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;

namespace UGem.Services.UserService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
   
    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
     
    }

    public async Task<Response.GetCustomerDetailsResponse> GetProfile()
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        
        var userIdGuid = Guid.Parse(userId!);
        
        var customer = await _dbContext.Customers
            .Include(customer => customer.User)
            .FirstOrDefaultAsync(x => x.UserId == userIdGuid);
        
        if (customer == null)
        {
            throw new Exception("Customer not found");
        }
        
        var result = new Response.GetCustomerDetailsResponse()
        {
            Id = customer.UserId,
            Name = customer.User.FullName,
            Email = customer.User.Email,
            PhoneNumber = customer.User.PhoneNumber,
            Role = customer.User.Role,
            AvatarUrl = customer.User.AvatarUrl
        };

        return result;
    }

    public async Task UpdateProfile(Request.UpdateProfileRequest request)
    {
        var userId = _httpContext.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "UserId")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("UserId not found");
        }
        
        var userIdGuid = Guid.Parse(userId);
        
        
        if (request.FullName != null && string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new Exception("Name cannot be empty");
        }

        if (request.AvatarUrl != null && string.IsNullOrWhiteSpace(request.AvatarUrl))
        {
            throw new Exception("AvatarUrl cannot be empty");
        }
        
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        if (request.FullName != null)
        {
            user.FullName = request.FullName.Trim();
        }

        if (request.AvatarUrl != null)
        {
            user.AvatarUrl = request.AvatarUrl;
        }
        
        await _dbContext.SaveChangesAsync();
    }
}