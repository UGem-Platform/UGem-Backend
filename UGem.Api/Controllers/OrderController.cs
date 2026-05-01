using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.Models;
using UGem.Services.OrderService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/order")]
public class OrderController : ControllerBase
{
    private readonly IService _orderService;

    public OrderController(IService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("merchant")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> GetOrderList()
    {
        var result = await _orderService.GetOrdersList();
        return Ok(ApiResponseFactory.SuccessResponse(result));
    }

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPost("{orderId}/accept")]
    public async Task<IActionResult> AcceptOrder(Guid orderId)
    {
        await _orderService.AcceptOrder(orderId);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Accept order success", HttpContext.TraceIdentifier));
    }
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPost("{orderId}/reject")]
    public async Task<IActionResult> RejectOrder([FromBody] Request.ReasonRejectRequest request)
    {
        await _orderService.RejectOrder(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Order rejected successfully", HttpContext.TraceIdentifier));
    }
    

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder(Request.CreateOrderRequest request)
    {
        await _orderService.CreateOrder(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Create order success", HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpGet("customer/list")]
    public async Task<IActionResult> GetOrderListFromCustomerId()
    {
        var result = await _orderService.GetOrderListFromCustomerId();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get order list success", HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpGet("{orderId}/detail")]
    public async Task<IActionResult> GetOrderDetail(Guid orderId)
    {
        var result = await _orderService.GetOrderDetail(orderId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get order detail success", HttpContext.TraceIdentifier));
    }
    
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPut("{orderId}/confirm-received")]
    public async Task<IActionResult> ConfirmOrderReceived(Request.ConfirmOrderRequest request)
    {
        await _orderService.ConfirmOrderReceived(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Confirm order received success"));
    }

    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPut("{orderId}/confirm-not-received")]
    public async Task<IActionResult> ConfirmOrderNotReceived(Request.ConfirmOrderRequest request)
    {
        await _orderService.ConfirmOrderNotReceived(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Confirm order not received success"));
    }
}