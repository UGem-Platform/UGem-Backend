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
    
    [HttpPost]
    public IActionResult Create()
    {
        return Ok();
    }

    
    [HttpGet]
    public async Task<IActionResult> GetOrderList()
    {
        var result = await _orderService.GetOrdersList();
        return Ok(ApiResponseFactory.SuccessResponse(result));
    }


    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok();
        
    }
}
