namespace UGem.Services.AffiliateLinkService;

public class Response
{
    public class CreateAffiliateLinkResponse
    {
        public Guid AffiliateLinkId { get; set; }

        public required string LinkCode { get; set; }

        public required string Url { get; set; }

        public int ClickCount { get; set; }

        public bool IsActive { get; set; }
    }
}