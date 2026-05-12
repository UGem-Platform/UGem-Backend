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

    [HttpPost("")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> CreateOrder(OrderRequest.CreateOrderRequest request)
    {
        var results =  await _orderService.CreateOrder(request);
        return Ok(ApiResponseFactory.SuccessResponse(results, "Order created successfully", HttpContext.TraceIdentifier));
    }

    [HttpPost("merchant")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> CreateOrderForMerchant(OrderRequest.CreateMerchantOrderRequest request)
    {
        var results = await _orderService.CreateMerchantOrder(request);
        return Ok(ApiResponseFactory.SuccessResponse(results, "Merchant order created successfully", HttpContext.TraceIdentifier));
    }
    
    [HttpPost(template: "sepay/webhook")]
    public async Task<IActionResult> SepayWebhook(Request.SepayWebhookRequest request)
    {
        await _orderService.SepayWebhookHandler(request);
        return Ok(ApiResponseFactory.SuccessResponse("", "Webhook response", HttpContext.TraceIdentifier));
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
    
    [HttpGet("bill")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> GetBill([FromQuery] Request.GetBillByOrderIdRequest request)
    {
        var result = await _orderService.GetBill(request);
        return Ok(ApiResponseFactory.SuccessResponse(
            result,
            "Order bill retrieved successfully",
            HttpContext.TraceIdentifier
        ));
    }
    
    [HttpPost("bill/confirm")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> ConfirmBill(Request.ConfirmBillRequest request)
    {
        await _orderService.ConfirmBill(request);
        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Bill confirmed successfully",
            HttpContext.TraceIdentifier
        ));
    }
    
    [HttpPost("bill/reject")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> RejectBill([FromBody] OrderRequest.RejectBillRequest request)
    {
        await _orderService.RejectBill(request);

        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Bill rejected successfully",
            HttpContext.TraceIdentifier
        ));
    }
    [HttpPatch("bill")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> UpdateBill([FromBody] OrderRequest.UpdateBillRequest request)
    {
        var result = await _orderService.UpdateBill(request);
        return Ok(ApiResponseFactory.SuccessResponse(
            result,
            "Bill updated successfully",
            HttpContext.TraceIdentifier
        ));
    }
    [HttpPatch("{orderId}/cash/confirm")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> ConfirmCashPayment([FromRoute] Guid orderId)
    {
        await _orderService.ConfirmCashPayment(orderId);

        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Cash payment confirmed successfully",
            HttpContext.TraceIdentifier
        ));
    }
}