using Microsoft.AspNetCore.Mvc;
using UGem.Services.MerchantService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchantController : ControllerBase
{
    private readonly IService _service;

    public MerchantController(IService service)
    {
        _service = service;
    }

    [HttpGet("Merchants")]
    public async Task<IActionResult> Search(string? searchTerm, int pageSize, int pageIndex)
    {
        var result = await _service.Search(searchTerm, pageSize, pageIndex);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Merchants retrieved", HttpContext.TraceIdentifier));
    }
}