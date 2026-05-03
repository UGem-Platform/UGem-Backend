using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.CheckInService;

namespace UGem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CheckInController : ControllerBase
{
    private readonly IService _service;

    public CheckInController(IService service)
    {
        _service = service;
    }
    


        [Authorize(Policy = JwtExtensions.MerchantPolicy)]  
        [HttpGet("generate-qr")]
        public IActionResult GenerateQrCode(Guid orderId)
        {
            /*var qrText = $"order:{orderId}";*/
            var qrText = "https://youtu.be/z8UPAVTh2aE?si=p57PHNIFGyDAMdZB";
            var qrCodeBytes = _service.GenerateQrCode(qrText);
            return File(qrCodeBytes, "image/png");
        }
    }