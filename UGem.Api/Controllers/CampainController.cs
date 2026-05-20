using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.CampainService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/campaigns")]
public class CampaignController : ControllerBase
{
    private readonly IService _campaignService;

    public CampaignController(IService campaignService)
    {
        _campaignService = campaignService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _campaignService.GetCampaigns();

        return Ok(ApiResponseFactory.SuccessResponse(
            result,
            "Get campaign list successfully",
            HttpContext.TraceIdentifier));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCampaignById(Guid id)
    {
        var result = await _campaignService.GetCampaignById(id);

        return Ok(ApiResponseFactory.SuccessResponse(
            result,
            "Get campaign successfully",
            HttpContext.TraceIdentifier));
    }

    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminOrMerchantPolicy)]
    public async Task<IActionResult> CreateCampaign(
        [FromBody] Request.CreateCampaignRequest request)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new Exception("UserId not found");
        }

        var result = await _campaignService.CreateCampaign(
            request,
            Guid.Parse(userIdClaim));

        return Ok(ApiResponseFactory.SuccessResponse(
            result,
            "Create campaign successfully",
            HttpContext.TraceIdentifier));
    }

    [HttpPut]
    [Authorize(Policy = JwtExtensions.AdminOrMerchantPolicy)]
    public async Task<IActionResult> UpdateCampaign(
        [FromBody] Request.UpdateCampaignRequest request)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new Exception("UserId not found");
        }

        var result = await _campaignService.UpdateCampaign(
            request,
            Guid.Parse(userIdClaim));

        return Ok(ApiResponseFactory.SuccessResponse(
            result,
            "Update campaign successfully",
            HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = JwtExtensions.AdminOrMerchantPolicy)]
    public async Task<IActionResult> DeleteCampaign(Guid id)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new Exception("UserId not found");
        }

        var result = await _campaignService.DeleteCampaign(
            id,
            Guid.Parse(userIdClaim));

        return Ok(ApiResponseFactory.SuccessResponse(
            result,
            "Delete campaign successfully",
            HttpContext.TraceIdentifier));
    }
}