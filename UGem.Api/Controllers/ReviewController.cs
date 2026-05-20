using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.Models;
using UGem.Services.ReviewService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/reviews")]
public class ReviewController : ControllerBase
{
    private readonly IService _reviewService;

    public ReviewController(IService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("merchant")]
    [Authorize(Policy = JwtExtensions.CustomerOrReviewerPolicy)]
    public async Task<IActionResult> GetReviewByMerchantId([FromQuery] Request.GetReviewByMerchantIdRequest request)
    {
        var result = await _reviewService.GetReviewByMerchantId(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Reviews retrieved successfully", HttpContext.TraceIdentifier));
    }
    [HttpGet("merchant/review-details")]
    [Authorize(Policy = JwtExtensions.CustomerOrReviewerPolicy)]
    public async Task<IActionResult> GetReviewDetailsByMerchant([FromQuery]Request.GetReviewDetailsByMerchantRequest request)
    {
        var result = await _reviewService.GetReviewDetailsByMerchant(request);

        return Ok(ApiResponseFactory.SuccessResponse(
            result,
            "Review details retrieved successfully",
            HttpContext.TraceIdentifier
        ));
    }
    
    [HttpPost("merchant")]
    [Authorize(Policy = JwtExtensions.CustomerOrReviewerPolicy)]
    public async Task<IActionResult> ReviewMerchant([FromBody] Request.ReviewByMerchantIdRequest request)
    {
        await _reviewService.ReviewMerchant(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Review submitted successfully", HttpContext.TraceIdentifier));
    }
    
    [HttpPut("merchant")]
    public async Task<IActionResult> UpdateReviewMerchant([FromBody] Request.UpdateReviewByMerchantIdRequest request)
    {
        await _reviewService.UpdateReviewMerchant(request);

        return Ok(ApiResponseFactory.SuccessResponse(null, "Review updated successfully", HttpContext.TraceIdentifier));
    }
}
