using Microsoft.AspNetCore.Mvc;
using UGem.Services.Models;
using UGem.Services.OrderService;

namespace UGem.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OrderController: ControllerBase
{
    private readonly IService _service;

    public OrderController(IService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(Request.CreateOrderRequest request)
    {
        await _service.CreateOrder(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Create order success"));
    }
}
