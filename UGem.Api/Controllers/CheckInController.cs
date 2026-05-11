using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.CheckInService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/check-in")]
public class CheckInController : ControllerBase
{
    private readonly IService _service;

    public CheckInController(IService service)
    {
        _service = service;
    }

    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    [HttpGet("generate-qr")]
    public async Task<IActionResult> GenerateQrCode([FromQuery]Request.GenerateQrCodeRequest request)
    {

        var qrCodeBytes = await _service.GenerateQrCode(request);
        return File(qrCodeBytes, "image/png");
    }
    
    [HttpPost("verify")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> CreateCheckInForQr(Request.CreateCheckInForQr request)
    {
        await _service.CreateCheckInForQr(request);

        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Check-in recorded successfully",
            HttpContext.TraceIdentifier
        ));
    }
}
