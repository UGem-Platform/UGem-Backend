using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.Application;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly IService _applicationService;

    public ApplicationController(IService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet("staff")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetApplications([FromQuery] string? status = null)
    {
        var data = await _applicationService.GetApplications(status);
        return Ok(ApiResponseFactory.SuccessResponse(data));
    }

    [HttpPost("staff/{id:guid}/accept")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> AcceptApplication(Guid id)
    {
        var staffId = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "UserId")!.Value);

        await _applicationService.AcceptApplication(id, staffId);

        return Ok(ApiResponseFactory.SuccessResponse(null, "Application accepted"));
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateApplicationRequest(Request.ApplicationRequest request)
    {
        await _applicationService.CreateApplicationRequest(request);
        return Ok();
    }

    [HttpPut("resubmit")]
    public async Task<IActionResult> EditAfterReject(Request.UpdateApplicationRequest request)
    {
        var result = await _applicationService.EditApplicationAfterReject(request);

        return Ok(result);
    }

    [Authorize(Policy = JwtExtensions.AdminAndStaffPolicy)]
    [HttpPost("reject")]
    public async Task<IActionResult> Reject(Request.RejectApplicationRequest request)
    {
        var result = await _applicationService.RejectApplication(request);

        return Ok(result);
    }
}
