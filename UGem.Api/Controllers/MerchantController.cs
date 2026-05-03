using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.MerchantService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/merchants")]
public class MerchantController : ControllerBase
{
    private readonly IService _service;

    public MerchantController(IService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] Request.SearchRequest request)
    {
        var result = await _service.Search(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Merchants retrieved", HttpContext.TraceIdentifier));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _service.GetDetail(id);
        if (result == null)
        {
            return NotFound(ApiResponseFactory.ErrorResponse("Merchant not found", HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponseFactory.SuccessResponse(result, "Merchant detail retrieved", HttpContext.TraceIdentifier));
    }

    [HttpGet("by-category")]
    public async Task<IActionResult> GetMerchantByCategory([FromQuery] Request.GetByCategoryRequest request)
    {
        var result = await _service.GetMerchantByCategory(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Merchants by category retrieved",
            HttpContext.TraceIdentifier));
    }
    
    [HttpGet("map")]
    public async Task<IActionResult> Map([FromQuery] Request.MapRequest request)
    {
        var result = await _service.MapRequest(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Merchants for map retrieved",
            HttpContext.TraceIdentifier));
    }


}