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
    public async Task<IActionResult> CreateApplicationRequest(Request.ApplicationRequest request)
    {
        await _applicationService.CreateApplicationRequest(request);
        return Ok();
    }
    
    [HttpPut("")]
    public async Task<IActionResult> EditAfterReject(Request.UpdateApplicationRequest request)
    {
        var result = await _applicationService.EditApplicationAfterReject(request);

        return Ok(result);
    }
    [HttpPost("")]
    public async Task<IActionResult> Reject(Request.RejectApplicationRequest request)
    {
        var result = await _applicationService.RejectApplication(request);;

        return Ok(result);
    }
}