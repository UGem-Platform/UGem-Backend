using Microsoft.AspNetCore.Mvc;
using UGem.Services.Models;
using UGem.Services.UserService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/user")]
public class UserController: ControllerBase
{
    private readonly IService _service;

    public UserController(IService service)
    {
        _service = service;
    }
    [HttpPatch("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] Request.UpdateProfileRequest request)
    {
        await _service.UpdateProfile(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Update profile success"));
    }
}