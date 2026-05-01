using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.Application;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/applications")]
public class ApplicationController : ControllerBase
{
    private readonly IService _applicationService;

    public ApplicationController(IService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet("me")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> GetMyApplications()
    {
        var data = await _applicationService.GetMyApplications();
        return Ok(ApiResponseFactory.SuccessResponse(data));
    }

    [HttpGet("")]
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
        try
        {
            await _applicationService.AcceptApplication(id);
            return Ok(ApiResponseFactory.SuccessResponse(null, "Application accepted", HttpContext.TraceIdentifier));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponseFactory.ErrorResponse(ex.Message, traceId: HttpContext.TraceIdentifier));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseFactory.ErrorResponse(ex.Message, traceId: HttpContext.TraceIdentifier));
        }
        catch (DbUpdateException ex)
        {
            return Conflict(ApiResponseFactory.ErrorResponse(
                "Failed to approve application because merchant data conflicts with existing records.",
                ex.InnerException?.Message ?? ex.Message,
                HttpContext.TraceIdentifier));
        }
    }

    [HttpPost ("")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> CreateApplicationRequest(Request.ApplicationRequest request)
    {
        await _applicationService.CreateApplicationRequest(request);
        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> EditAfterReject(Request.UpdateApplicationRequest request)
    {
        var result = await _applicationService.EditApplicationAfterReject(request);

        return Ok(result);
    }

    [Authorize(Policy = JwtExtensions.AdminAndStaffPolicy)]
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Request.RejectApplicationRequest request)
    {
        var result = await _applicationService.RejectApplication(request);

        return Ok(result);
    }
}
