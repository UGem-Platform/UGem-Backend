using Microsoft.AspNetCore.Mvc;
using UGem.Services.IdentityService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly IService _identityService;

    public IdentityController(IService identityService)
    {
        _identityService = identityService;
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var result = await _identityService.Login(email, password);
        return Ok(result);
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Request.RegisterUserRequest request)
    {
        var result = await _identityService.Register(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Register successfully"));
    }
}