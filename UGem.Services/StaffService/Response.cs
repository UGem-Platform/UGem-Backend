namespace UGem.Services.StaffService;

public class Response
{
    public class ReviewerApplicationResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = null!;
        public string Motivation { get; set; } = null!;
        public string? Experience { get; set; }

        public string? FacebookUrl { get; set; }
        public string? TiktokUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public string? OtherSocialUrl { get; set; }

        public string? RejectionReason { get; set; }

        public Guid CustomerId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}