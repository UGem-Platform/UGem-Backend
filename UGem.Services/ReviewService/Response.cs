namespace UGem.Services.ReviewService;

public class Response
{
    public record GetReviewsResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid OderId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}