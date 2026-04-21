using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UGem.Repositories;
using UGem.Service.JwtService;

namespace UGem.Service.Identity;

public class Service : IService
{
    private readonly JwtService.IService _jwtService;
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOption = new();

    public Service(IConfiguration configuration, JwtService.IService jwtService, AppDbContext dbContext)
    {
        _jwtService = jwtService;
        _dbContext = dbContext;
        configuration.GetSection(nameof(JwtOptions)).Bind(_jwtOption);
    }

    public async Task<Response.IdentityResponse> Login(string phoneNumber, string password)
    {
        var user = await _dbContext.Users
            .Include(x => x.Customer)
            .Include(x => x.Staff)    
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        if(user.PasswordHash != password)
        {
                throw new Exception("Invalid password");
        }
        
        
        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Email", user.Email),
            new Claim("Role", user.Role),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Expired, 
                DateTimeOffset.UtcNow.AddMinutes(_jwtOption.ExpireMinutes).ToString()),
        };
        
        
        if (user.Role == "Customer")
        {
            claims.Add(new Claim("CustomerId", user.Customer!.Id.ToString()));

        }
        
        
        var token = _jwtService.GenerateAccessToken(claims);
        
            var result = new Response.IdentityResponse()
            {
            AccessToken = token
        };

        return result;
    }
}