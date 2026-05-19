using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Services.IdentityService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class IdentityController : ControllerBase
{
    private readonly IService _identityService;

    public IdentityController(IService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Request.LoginRequest request)
    {
        var result = await _identityService.Login(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Login successful", HttpContext.TraceIdentifier));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Request.RegisterUserRequest request)
    {
        var result = await _identityService.Register(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Register successfully", HttpContext.TraceIdentifier));
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] Request.GoogleLoginRequest request)
    {
        var result = await _identityService.GooleLogin(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Google Login success", HttpContext.TraceIdentifier));
    }

    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        var userIdValue = User.FindFirstValue("UserId");

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid token");
        }

        var result = await _identityService.RefreshToken(userId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Token refreshed", HttpContext.TraceIdentifier));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] Request.ForgotPasswordRequest request)
    {
        await  _identityService.ForgotPassword(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "OTP send to your email", HttpContext.TraceIdentifier));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] Request.ResetPasswordRequest request)
    {
        await _identityService.ResetPassword(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Password reset Successfully", HttpContext.TraceIdentifier));
    }
}
