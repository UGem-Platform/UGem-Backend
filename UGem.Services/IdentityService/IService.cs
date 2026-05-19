namespace UGem.Services.IdentityService;

public interface IService
{
    public Task<Response.IdentityResponse> Login(Request.LoginRequest request);
    public Task<string> Register(Request.RegisterUserRequest request);
    public Task<Response.IdentityResponseGoogle> GooleLogin(Request.GoogleLoginRequest request);
    public Task ForgotPassword (Request.ForgotPasswordRequest request);
    public Task ResetPassword(Request.ResetPasswordRequest request);
}