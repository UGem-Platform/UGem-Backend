using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Services.MediaService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/v1/media")]
public class MediaController : ControllerBase
{
    private readonly IService _mediaService;

    public MediaController(IService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost("images")]
    [Authorize]
    [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        var imageUrl = await _mediaService.UploadImageAsync(file);

        return Ok(new
        {
            url = imageUrl
        });
    }
}
