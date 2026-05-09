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
    [HttpGet("generate-qr/{orderId}")]
    public IActionResult GenerateQrCode(Guid orderId)
    {
        var qrText = "https://www.youtube.com/watch?v=XWt96eZphlU";
        var qrCodeBytes = _service.GenerateQrCode(qrText);
        return File(qrCodeBytes, "image/png");
    }
    
    [HttpPost("verify")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> FireCheckIn()
    {
        await _service.FireCheckIn();

        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Check-in recorded successfully",
            HttpContext.TraceIdentifier
        ));
    }
}