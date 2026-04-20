using System.Security.Claims;

namespace UGem.Service.JwtService;

public interface IService
{
    public string GenerateAccessToken(IEnumerable<Claim> claims);

    ClaimsPrincipal ValidateToken(string token);
}