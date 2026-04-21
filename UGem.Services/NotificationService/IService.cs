namespace UGem.Services.NotificationService;

public interface IService
{
    public Task<List<Response.NotificationResponse>> GetNotificationRequests();
}