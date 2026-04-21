namespace UGem.Services.NotificationService;

public class Response
{
    public class NotificationResponse
    {
        public required string Title { get; set; }
        
        public required string Message { get; set; }
        
        public bool IsRead { get; set; }
    }
}