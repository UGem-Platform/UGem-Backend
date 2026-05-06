using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.FoodService;
using UGem.Services.Models;
using IService = UGem.Services.FoodService.IService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/foods")]
public class FoodController : ControllerBase
{
    private readonly IService _foodService;

    public FoodController(IService foodService)
    {
        _foodService = foodService;
    }

    [HttpPost]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task< IActionResult> Create( [FromBody]Request.AddFoodRequest request)
    {
        await _foodService.CreateFood(request );
        return Ok(ApiResponseFactory.SuccessResponse(null, "Add food Successfully", HttpContext.TraceIdentifier));
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok();
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> DeleteById(Guid id)
    {
        await _foodService.DeleteFood(id);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Delete Food Successfully", HttpContext.TraceIdentifier));
    }
}