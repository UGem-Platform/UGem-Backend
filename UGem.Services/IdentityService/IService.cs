namespace UGem.Services.IdentityService;

public interface IService
{
    public Task<Response.IdentityResponse> Login(Request.LoginRequest request);
    public Task<string> Register(Request.RegisterUserRequest request);
}