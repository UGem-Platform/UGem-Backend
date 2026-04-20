using Microsoft.AspNetCore.Mvc;

namespace UGem.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class MerchantController: ControllerBase
{
    [HttpPost]
    public IActionResult Create()
    {
        return Ok();
    }

    // GET /merchant
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok();
    }

    // GET /merchant/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        return Ok();
    }
}