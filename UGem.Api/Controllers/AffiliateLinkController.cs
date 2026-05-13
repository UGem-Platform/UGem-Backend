using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.AffiliateLinkService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/affiliate-links")]
public class AffiliateLinkController : ControllerBase
{
    private readonly IService _service;

    public AffiliateLinkController(IService service)
    {
        _service = service;
    }

    [Authorize(Policy = JwtExtensions.ReviewerPolicy)]
    [HttpPost("")]
    public async Task<IActionResult> CreateAffiliateLink(
        Request.CreateAffiliateLinkRequest request)
    {
        var result = await _service.CreateAffiliateLink(request);

        return Ok(ApiResponseFactory.SuccessResponse(result, "Create affiliate link success"));
    }
}