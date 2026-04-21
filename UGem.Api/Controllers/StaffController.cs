using Microsoft.AspNetCore.Mvc;
using UGem.Service.StaffService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StaffController : ControllerBase
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