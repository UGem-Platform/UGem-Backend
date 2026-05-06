namespace UGem.Services.ReviewService;

public class Response
{
    public class GetReviewsResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid OderId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
    
    public class ReviewsByIdMerchantResponse
    {
        public int Rating { get; set; }
        public string Content { get; set; } = "";
        public DateTimeOffset CreatedAt { get; set; }
        
        public List<ReviewDetailsResponse>? ReviewDetails { get; set; } 

    }
    
    public class ReviewDetailsResponse
    {
        public string? DetailContent { get; set; } 
        public int? Rating { get; set; } 
    }
    
    public class ReviewDetailResponse
    {
        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }
        public Guid OrderDetailId { get; set; }
        public string? DetailContent { get; set; }
        public int Rating { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}