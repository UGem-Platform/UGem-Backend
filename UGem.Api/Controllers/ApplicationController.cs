using Microsoft.AspNetCore.Mvc;
using UGem.Services.Application;

namespace UGem.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly IService _service;

    public ApplicationController(IService service)
    {
        _service = service;
    }
    
    [HttpPut("")]
    public async Task<IActionResult> EditAfterReject(Request.UpdateApplicationRequest request)
    {
        var result = await _service.EditApplicationAfterReject(request);

        return Ok(result);
    }
    [HttpPost("")]
    public async Task<IActionResult> Reject(Request.RejectApplicationRequest request)
    {
        var result = await _service.RejectApplication(request);

        return Ok(result);
    }
}