using Microsoft.AspNetCore.Mvc;

namespace UGem.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class CategoryController: ControllerBase
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