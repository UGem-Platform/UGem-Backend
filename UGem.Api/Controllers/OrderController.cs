using Microsoft.AspNetCore.Mvc;

namespace UGem.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class OrderController: ControllerBase
{
    [HttpPost]
    public IActionResult Create()
    {
        return Ok();
    }

    // GET /category
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok();
    }

    // GET /category/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok();
        
    }
}