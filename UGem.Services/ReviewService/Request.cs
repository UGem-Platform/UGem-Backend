namespace UGem.Services.ReviewService;

public class Request
{
    
    public class GetReviewByMerchantIdRequest
    {
        public Guid MerchantId { get; set; }
    }
    public class ReviewByMerchantIdRequest
    {
        public Guid MerchantId { get; set; }
        public Guid OrderId { get; set; }
        public int Rating { get; set; }
        public required string Content { get; set; } 
        public string? ImageUrl { get; set; }
        
        public List<ReviewDetailsRequest>? ReviewDetails { get; set; } 

    }
    public class ReviewDetailsRequest
    {
        public Guid OrderDetailId { get; set; }
        public string? DetailContent { get; set; } = "";
        public int Rating { get; set; } = 0;
    }
    public class UpdateReviewByMerchantIdRequest
    {
        public Guid ReviewId { get; set; }
        public int? Rating { get; set; }
        public string? Content { get; set; } 
        public string? ImageUrl { get; set; }
        
        public List<UpdateReviewDetailsRequest>? ReviewDetails { get; set; } 

    }
    public class UpdateReviewDetailsRequest
    {
        public Guid ReviewDetailId { get; set; }
        public string? DetailContent { get; set; } 
        public int? Rating { get; set; } 
    }
    

}