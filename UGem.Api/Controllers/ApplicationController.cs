using Microsoft.AspNetCore.Mvc;
using UGem.Services.Application;

namespace UGem.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly IService _applicationService;
    public ApplicationController(IService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpPost("CreateApplicationRequest")]
    public async Task<IActionResult> CreateApplicationRequest(Request.CreateApplicationRequest request)
    {
        await _applicationService.CreateApplicationRequest(request);
        return Ok();
    }
    
}