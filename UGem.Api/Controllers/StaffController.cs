using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.Models;
using UGem.Services.StaffService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/staff")]
public class StaffController : ControllerBase
{
    private readonly IService _service;

    public StaffController(IService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Create()
    {
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetReviewerApplications([FromQuery] string? searchTerm, int pageSize = 10, int pageIndex = 1)
    {
        var result = await _service.GetReviewerApplications(searchTerm, pageSize, pageIndex);
        return Ok(ApiResponseFactory.SuccessResponse(result, "GetReviewerApplications Successfully", HttpContext.TraceIdentifier));
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok();
    }
    [HttpPost("accept")]
    [Authorize(Policy = JwtExtensions.StaffPolicy)]
    public async Task<IActionResult> Approve([FromBody] Request.ApproveReviewerApplicationRequest request)
    
    {
        await _service.ApproveApplication(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Approve Successfully", HttpContext.TraceIdentifier));
    }

    [HttpPost("reject")]
    [Authorize(Policy = JwtExtensions.StaffPolicy)]
    public async Task<IActionResult> Reject([FromBody] Request.RejectReviewerApplicationRequest request)
    {
        await _service.RejectApplication(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Reject Successfully", HttpContext.TraceIdentifier));
    }
}