namespace UGem.Services.ReviewerApplicationService;

public class Request
{
    public class ReviewerApplicationRequest
    {
        public required string Motivation { get; set; }
        public string? Experience { get; set; }
        public string? FacebookUrl { get; set; }
        public string? TiktokUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public string? OtherSocialUrl { get; set; }
    }
    
    public class UpdateReviewerApplicationRequest
    {
        public Guid reviewerApplicationId { get; set; }
        public string? Motivation { get; set; }
        public string? Experience { get; set; }
        public string? FacebookUrl { get; set; }
        public string? TiktokUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public string? OtherSocialUrl { get; set; }
    }
}