using Microsoft.AspNetCore.Mvc;
using UGem.Repositories;
using UGem.Services.Category;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IService _categoryService;

    public CategoryController(AppDbContext dbContext, IService categoryService)
    {
        _dbContext = dbContext;
        _categoryService = categoryService;
    }

    [HttpPost]
    public IActionResult Create()
    {
        return Ok();
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
}