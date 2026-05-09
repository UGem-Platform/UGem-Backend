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
    public IActionResult GenerateQrCode()
    {
        var qrText = $"https://u-gem.vercel.app/check-in";
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