namespace UGem.Services.ReviewerApplicationService;

public class Response
{
    public class GetReviewApplicationResponse
    {
        public required string Motivation { get; set; }
        public string? Experience { get; set; }
        public string? FacebookUrl { get; set; }
        public string? TiktokUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public string? OtherSocialUrl { get; set; }
    }
}