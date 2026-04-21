using System.Security.Claims;

namespace UGem.Services.JwtService;

public interface IService
{
    public string GenerateAccessToken(IEnumerable<Claim> claims);

    ClaimsPrincipal ValidateToken(string token);
}