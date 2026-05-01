using Microsoft.AspNetCore.Mvc;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class AffiliateLinkController : ControllerBase
{
    [HttpPost]
    public IActionResult Create()
    {
        return Ok();
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
}