using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.CustomerService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly IService _service;

    public CustomerController(IService service)
    {
        _service = service;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _service.GetProfile();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get profile success"));
    }

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPut("confirm-received")]
    public async Task<IActionResult> ConfirmOrderReceived(Request.ConfirmOrderRequest request)
    {
        await _service.ConfirmOrderReceived(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Confirm order received success"));
    }

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPut("confirm-not-received")]
    public async Task<IActionResult> ConfirmOrderNotReceived(Request.ConfirmOrderRequest request)
    {
        await _service.ConfirmOrderNotReceived(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Confirm order not received success"));
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Request.RegisterCustomerRequest request)
    {
        var result = await _service.CreateCustomer(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Register success"));
    }
}