using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.Models;
using UGem.Services.OrderService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IService _orderService;

    public OrderController(IService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> GetOrderList()
    {
        var result = await _orderService.GetOrdersList();
        return Ok(ApiResponseFactory.SuccessResponse(result));
    }

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPost("accept")]
    public async Task<IActionResult> AcceptOrder(Guid orderId)
    {
        await _orderService.AcceptOrder(orderId);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Accept order success", HttpContext.TraceIdentifier));
    }
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPost("reject")]
    public async Task<IActionResult> RejectOrder([FromBody] Request.ReasonRejectRequest request)
    {
        await _orderService.RejectOrder(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Order rejected successfully", HttpContext.TraceIdentifier));
    }
    

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPost("customer/orders")]
    public async Task<IActionResult> CreateOrder(Request.CreateOrderRequest request)
    {
        await _orderService.CreateOrder(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Create order success", HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpGet("list")]
    public async Task<IActionResult> GetOrderListFromCustomerId()
    {
        var result = await _orderService.GetOrderListFromCustomerId();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get order list success", HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpGet("detail")]
    public async Task<IActionResult> GetOrderDetail(Guid orderId)
    {
        var result = await _orderService.GetOrderDetail(orderId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get order detail success", HttpContext.TraceIdentifier));
    }
    
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPut("confirm-received")]
    public async Task<IActionResult> ConfirmOrderReceived(Request.ConfirmOrderRequest request)
    {
        await _orderService.ConfirmOrderReceived(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Confirm order received success"));
    }

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPut("confirm-not-received")]
    public async Task<IActionResult> ConfirmOrderNotReceived(Request.ConfirmOrderRequest request)
    {
        await _orderService.ConfirmOrderNotReceived(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Confirm order not received success"));
    }
}