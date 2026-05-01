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

    [HttpGet("merchant/applications")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> GetMyApplications()
    {
        var data = await _applicationService.GetMyApplications();
        return Ok(ApiResponseFactory.SuccessResponse(data));
    }

    [HttpGet("staff/applications")]
    [Authorize(Policy = JwtExtensions.AdminAndStaffPolicy)]
    public async Task<IActionResult> GetApplications()
    {
        var data = await _applicationService.GetApplications();
        return Ok(ApiResponseFactory.SuccessResponse(data));
    }

    [HttpPost("{id}/accept")]
    [Authorize(Policy = JwtExtensions.AdminAndStaffPolicy)]
    public async Task<IActionResult> AcceptApplication(Guid id)
    {
        await _applicationService.AcceptApplication(id);

        return Ok(ApiResponseFactory.SuccessResponse(null, "Application accepted"));
    }

    [HttpPost ("merchant/applications/create")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> CreateApplicationRequest(Request.ApplicationRequest request)
    {
        await _applicationService.CreateApplicationRequest(request);
        return Ok();
    }

    [HttpPut("resubmit")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
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
