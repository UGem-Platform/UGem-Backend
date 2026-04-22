using Microsoft.AspNetCore.Mvc;
using UGem.Services.Models;
using UGem.Services.OrderService;

namespace UGem.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OrderController: ControllerBase
{
    private readonly IService _orderService;
    public OrderController(IService orderService)
    {
        _orderService = orderService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetOrderList()
    {
        var result = await _orderService.GetOrdersList();
        return Ok(ApiResponseFactory.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(Request.CreateOrderRequest request)
    {
        await _orderService.CreateOrder(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Create order success"));
    }
}
