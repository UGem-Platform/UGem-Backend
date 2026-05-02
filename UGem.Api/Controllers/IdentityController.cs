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
        // var result = await _identityService.Login(request);
        // return Ok(ApiResponseFactory.SuccessResponse(result, "Login successful", HttpContext.TraceIdentifier));
        try
        {
            var result = await _identityService.Login(request);

            return Ok(ApiResponseFactory.SuccessResponse(
                result,
                "Login successful",
                HttpContext.TraceIdentifier
            ));
        }
        catch (Exception ex)
        {
            Console.WriteLine("LOGIN ERROR:");
            Console.WriteLine(ex.ToString());

            return StatusCode(500,
                ApiResponseFactory.ErrorResponse(
                    "Internal server error",
                    ex.Message,
                    HttpContext.TraceIdentifier
                ));
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Request.RegisterUserRequest request)
    {
        var result = await _identityService.Register(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Register successfully", HttpContext.TraceIdentifier));
    }
}