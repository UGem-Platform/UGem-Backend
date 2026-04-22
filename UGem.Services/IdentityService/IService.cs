namespace UGem.Services.IdentityService;

public interface IService
{
    public Task<Response.IdentityResponse> Login(string email, string password);

}