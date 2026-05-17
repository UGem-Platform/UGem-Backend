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
        try
        {
            var imageUrl = await _mediaService.UploadImageAsync(file);

            return Ok(new
            {
                url = imageUrl
            });
        }
        catch (Exception ex)
        {
            // Fallback to local storage if Cloudinary fails
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "images");
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Ensure the URL is accessible via static files
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            var localUrl = $"{baseUrl}/uploads/images/{fileName}";

            return Ok(new
            {
                url = localUrl,
                note = "Uploaded to local storage due to Cloudinary failure.",
                debugInfo = new
                {
                    directoryExists = Directory.Exists(uploadsFolder),
                    filePath = filePath,
                    webRoot = webRootPath
                },
                error = ex.Message
            });
        }
    }
}
