using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Services.Models;
using UGem.Services.NotificationService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("api/notification")]
public class NotificationController : ControllerBase
{
    private readonly IService _notificationService;

    public NotificationController(IService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("list")]
    [Authorize]
    public async Task<IActionResult> GetNotifications()
    {
        var result = await _notificationService.GetNotificationRequests();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Notifications retrieved", HttpContext.TraceIdentifier));
    }
}
