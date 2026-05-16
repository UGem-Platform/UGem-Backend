namespace UGem.Services.NotificationService;

public class Response
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }

        public required string Title { get; set; }
        
        public required string Message { get; set; }

        public required string Type { get; set; }
        
        public bool IsRead { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}