using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.Models;
using UGem.Services.ReviewerApplicationService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/reviewer-application")]
public class ReviewerApplication : ControllerBase
{
    private readonly IService _reviewerService;

    public ReviewerApplication(IService service)
    {
        _reviewerService = service; 
    }

    [HttpPost("")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> CreateReviewerApplication([FromBody] Request.ReviewerApplicationRequest request)
    {
        await _reviewerService.CreateReviewerApplication(request);

        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Reviewer application submitted successfully",
            HttpContext.TraceIdentifier
        ));
    }
    [HttpPatch("")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> UpdateReviewerApplication([FromBody]Request.UpdateReviewerApplicationRequest request)
    {
        await _reviewerService.UpdateReviewerApplication(request);

        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Reviewer application updated successfully",
            HttpContext.TraceIdentifier
        ));
    }
    
    [HttpGet("")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> GetMyReviewerApplication()
    {
        var result = await _reviewerService.GetReviewApplicationByCus();

        return Ok(ApiResponseFactory.SuccessResponse(
            result,
            "Reviewer application retrieved successfully",
            HttpContext.TraceIdentifier
        ));
    }
}