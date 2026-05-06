using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.Application;
using UGem.Services.Models;
using ApplicationRequest = UGem.Services.Application.Request;

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

    [HttpGet("mine")]
    [Authorize(Policy = JwtExtensions.MerchantApplicantPolicy)]
    public async Task<IActionResult> GetMyApplications()
    {
        var data = await _applicationService.GetMyApplications();
        return Ok(ApiResponseFactory.SuccessResponse(data, "Merchant applications retrieved", HttpContext.TraceIdentifier));
    }

    [HttpGet]
    [Authorize(Policy = JwtExtensions.AdminAndStaffPolicy)]
    public async Task<IActionResult> GetApplications()
    {
        var data = await _applicationService.GetApplications();
        return Ok(ApiResponseFactory.SuccessResponse(data, "Applications retrieved", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    [Authorize(Policy = JwtExtensions.MerchantApplicantPolicy)]
    public async Task<IActionResult> CreateApplicationRequest([FromForm] Request.ApplicationRequest request)
    {
        await _applicationService.CreateApplicationRequest(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Application submitted", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = JwtExtensions.MerchantApplicantPolicy)]
    public async Task<IActionResult> EditAfterReject(Guid id, Request.UpdateApplicationRequest request)
    {
        request.ApplicationId = id;
        var result = await _applicationService.EditApplicationAfterReject(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Application updated", HttpContext.TraceIdentifier));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Policy = JwtExtensions.AdminAndStaffPolicy)]
    public async Task<IActionResult> UpdateApplicationStatus(Guid id, ApplicationRequest.UpdateApplicationStatusRequest request)
    {
        try
        {
            switch (request.Status)
            {
                case ApplicationRequest.ApplicationStatus.Accepted:
                    await _applicationService.AcceptApplication(id);
                    return Ok(ApiResponseFactory.SuccessResponse(null, "Application accepted", HttpContext.TraceIdentifier));
                case ApplicationRequest.ApplicationStatus.Rejected:
                    await _applicationService.RejectApplication(new ApplicationRequest.RejectApplicationRequest { ApplicationId = id, Note = request.Note ?? string.Empty });
                    return Ok(ApiResponseFactory.SuccessResponse(null, "Application rejected", HttpContext.TraceIdentifier));
default:
                    return BadRequest(ApiResponseFactory.ErrorResponse($"Unsupported application status '{request.Status}'", traceId: HttpContext.TraceIdentifier));
            }
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
                "Failed to update application because merchant data conflicts with existing records.",
                ex.InnerException?.Message ?? ex.Message,
                HttpContext.TraceIdentifier));
        }
    }
}