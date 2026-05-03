using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.CheckInService;

namespace UGem.Api.Controllers;

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
}