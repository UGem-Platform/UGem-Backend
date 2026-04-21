using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Services.Application;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly IService _applicationService;
    [HttpGet("staff")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetApplications([FromQuery] string? status = null)
    {
        var data = await _applicationService.GetApplications(status);
        return Ok(ApiResponseFactory.SuccessResponse(data));
    }
    [HttpPost("staff/{id}/accept")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> AcceptApplication(Guid id)
    {
        var staffId = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "UserId")!.Value);
 
        await _applicationService.AcceptApplication(id, staffId);
 
        return Ok(ApiResponseFactory.SuccessResponse(null, "Application accepted"));
    }
}