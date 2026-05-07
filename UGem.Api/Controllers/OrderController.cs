using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.Models;
using UGem.Services.OrderService;
using OrderRequest = UGem.Services.OrderService.Request;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrderController : ControllerBase
{
    private readonly IService _orderService;

    public OrderController(IService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> GetOrdersForMerchant()
    {
        var result = await _orderService.GetOrdersList();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Orders retrieved", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> CreateOrder(OrderRequest.CreateOrderRequest request)
    {
        await _orderService.CreateOrder(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Order created successfully", HttpContext.TraceIdentifier));
    }

    [HttpGet("mine")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> GetMyOrders()
    {
        var result = await _orderService.GetOrderListFromCustomerId();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Customer orders retrieved", HttpContext.TraceIdentifier));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> GetOrderDetail(Guid id)
    {
        var result = await _orderService.GetOrderDetail(id);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Order detail retrieved", HttpContext.TraceIdentifier));
    }

    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, OrderRequest.UpdateOrderStatusRequest request)
    {
        try
        {
            switch (request.Status)
            {
                case OrderRequest.OrderStatus.Accepted:
                    await _orderService.AcceptOrder(id);
                    return Ok(ApiResponseFactory.SuccessResponse(null, "Order accepted", HttpContext.TraceIdentifier));
                case OrderRequest.OrderStatus.Rejected:
                    if (string.IsNullOrWhiteSpace(request.Reason))
                    {
                        return BadRequest(ApiResponseFactory.ErrorResponse("Reason is required for rejecting an order",
                            traceId: HttpContext.TraceIdentifier));
                    }

                    await _orderService.RejectOrder(new OrderRequest.ReasonRejectRequest
                        { OrderId = id, Reason = request.Reason });
                    return Ok(ApiResponseFactory.SuccessResponse(null, "Order rejected", HttpContext.TraceIdentifier));
                case OrderRequest.OrderStatus.Completed:
                    await _orderService.ConfirmOrderReceived(new OrderRequest.ConfirmOrderRequest { OrderId = id });
                    return Ok(ApiResponseFactory.SuccessResponse(null, "Order marked as completed",
                        HttpContext.TraceIdentifier));
                case OrderRequest.OrderStatus.NotReceived:
                    await _orderService.ConfirmOrderNotReceived(new OrderRequest.ConfirmOrderRequest { OrderId = id });
                    return Ok(ApiResponseFactory.SuccessResponse(null, "Order marked as not received",
                        HttpContext.TraceIdentifier));
                default:
                    return BadRequest(ApiResponseFactory.ErrorResponse($"Unsupported status '{request.Status}'",
                        traceId: HttpContext.TraceIdentifier));
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseFactory.ErrorResponse(ex.Message, traceId: HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("{orderId}/accept")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> AcceptOrder([FromRoute] Guid orderId)
    {
        await _orderService.AcceptOrder(orderId);

        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Order accepted successfully",
            HttpContext.TraceIdentifier
        ));
    }

    [HttpPost("reject")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> RejectOrder(Request.ReasonRejectRequest request)
    {
        await _orderService.RejectOrder(request);

        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Order rejected successfully",
            HttpContext.TraceIdentifier
        ));
    }
}