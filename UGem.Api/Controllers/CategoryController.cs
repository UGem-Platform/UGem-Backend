using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.Category;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoryController : ControllerBase
{
    private readonly IService _categoryService;

    public CategoryController(IService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var listResult = await _categoryService.GetCategories();
        return Ok(ApiResponseFactory.SuccessResponse(listResult, "Categories list", HttpContext.TraceIdentifier));
    }


    [HttpGet("{parentId}/children")]
    public async Task<IActionResult> GetById([FromRoute] Guid parentId)
    {
        var listResult = await _categoryService.GetCategoryById(parentId);
        return Ok(ApiResponseFactory.SuccessResponse(listResult, "Child Categories retrieved",
            HttpContext.TraceIdentifier));
    }

    [HttpPost("")]
    [Authorize(Policy = JwtExtensions.StaffPolicy)]
    public async Task<IActionResult> Createcategory([FromBody]Request.CreateCategoryRequest request)
    {
        var result = await _categoryService.AddCategory(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Category added successfully", HttpContext.TraceIdentifier));
    }
}