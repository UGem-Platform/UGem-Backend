using Microsoft.AspNetCore.Mvc;
using UGem.Services.Models;
using UGem.Services.NotificationService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly IService _notificationService;
    
    public NotificationController(IService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var result = await _notificationService.GetNotificationRequests();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Notifications retrieved", HttpContext.TraceIdentifier));
    }
}
