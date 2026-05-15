using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.AdminService;
using UGem.Services.Models;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private readonly IService _adminService;

    public AdminController(IService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("staff")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetAllStaff([FromQuery] string? searchTerm, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 1)
    {
        var result = await _adminService.GetAllStaffForAdmin(searchTerm, pageSize, pageIndex);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get staff list success"));
    }

    [HttpPost("staff")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateStaff([FromBody] Request.CreateStaffRequest request)
    {
        await _adminService.CreateStaff(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Create staff success"));
    }

    [HttpDelete("staff/{staffId}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteStaff(Guid staffId)
    {
        await _adminService.DeleteStaff(staffId);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Delete staff success"));
    }
    [HttpGet("dashboard")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.GetDashboard();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get dashboard success"));
    }
    [HttpGet("merchant-revenues")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetMerchantRevenues()
    {
        var result = await _adminService.GetMerchantRevenues();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get merchant revenues success"));
    }
    
    [HttpGet("merchant-revenues/{merchantId}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetMerchantDetail(
        Guid merchantId,
        [FromQuery] string periodType = "Month")
    {
        var result = await _adminService.GetMerchantDetail(merchantId, periodType);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get merchant detail success"));
    }
}