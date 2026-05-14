using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.FoodToppingService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/food-toppings")]
public class FoodToppingController : ControllerBase
{
    private readonly IService _foodToppingService;

    public FoodToppingController(IService foodToppingService)
    {
        _foodToppingService = foodToppingService;
    }

    [HttpPost]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> Create(
        [FromBody] Request.CreateFoodToppingRequest request)
    {
        await _foodToppingService.CreateFoodTopping(request);

        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Create food topping successfully",
            HttpContext.TraceIdentifier));
    }

    [HttpGet("{foodId}/toppings")]
    [Authorize(Policy = JwtExtensions.MerchantAndCustomer)]
    public async Task<IActionResult> GetFoodToppings(Guid foodId)
    {
        var result = await _foodToppingService.GetFoodToppings(foodId);

        return Ok(ApiResponseFactory.SuccessResponse(
            result,
            "Get food toppings successfully",
            HttpContext.TraceIdentifier));
    }

    [HttpPut]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> Update(
        [FromBody] Request.UpdateFoodToppingRequest request)
    {
        await _foodToppingService.UpdateFoodTopping(request);

        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Update food topping successfully",
            HttpContext.TraceIdentifier));
    }

    [HttpDelete("{foodToppingId}")]
    [Authorize(Policy = JwtExtensions.MerchantPolicy)]
    public async Task<IActionResult> DeleteFoodTopping(Guid foodToppingId)
    {
        await _foodToppingService.DeleteFoodTopping(foodToppingId);

        return Ok(ApiResponseFactory.SuccessResponse(
            null,
            "Delete food topping successfully",
            HttpContext.TraceIdentifier));
    }
}