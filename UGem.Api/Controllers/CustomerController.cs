using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.CustomerService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/customers")]
public class CustomerController : ControllerBase
{
    private readonly IService _service;

    public CustomerController(IService service)
    {
        _service = service;
    }

    [HttpGet("search-by-email")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> SearchUserByEmail([FromQuery] string? email, [FromQuery] int limit = 10)
    {
        var result = await _service.SearchUserByEmail(email, limit);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Search user by email success"));
    }
    
}

