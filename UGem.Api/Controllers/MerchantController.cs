using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.MerchantService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchantController : ControllerBase
{
    private readonly IService _service;
    private readonly UGem.Services.QRCodeService.IService _qrCodeService;

    public MerchantController(IService service, UGem.Services.QRCodeService.IService qrCodeService)
    {
        _service = service;
        _qrCodeService = qrCodeService;
    }

    [HttpGet("Merchants")]
    public async Task<IActionResult> Search(Request.SearchRequest request)
    {
        var result = await _service.Search(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Merchants retrieved", HttpContext.TraceIdentifier));
    }

    [HttpGet("Merchants/{id}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _service.GetDetail(id);
        if (result == null)
        {
            return NotFound(ApiResponseFactory.ErrorResponse("Merchant not found", HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponseFactory.SuccessResponse(result, "Merchant detail retrieved", HttpContext.TraceIdentifier));
    }

    [HttpGet("Category/Merchants")]
    public async Task<IActionResult> GetMerchantByCategory(Request.GetByCategoryRequest request)
    {
        var result = await _service.GetMerchantByCategory(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Merchants by category retrieved",
            HttpContext.TraceIdentifier));
    }
    
    [HttpGet("Map/Merchants")]
    public async Task<IActionResult> Map(Request.MapRequest request)
    {
        var result = await _service.MapRequest(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Merchants for map retrieved",
            HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    [HttpGet("generate-qr/{orderId}")]
    public IActionResult GenerateQrCode(Guid orderId)
    {
        var qrText = "https://www.youtube.com/watch?v=XWt96eZphlU";
        var qrCodeBytes = _qrCodeService.GenerateQrCode(qrText);
        return File(qrCodeBytes, "image/png");
    }
}