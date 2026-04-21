namespace UGem.Services.IdentityService;

public interface IService
{
    public Task<Response.IdentityResponse> Login(string phoneNumber, string password);

}