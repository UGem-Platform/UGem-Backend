namespace UGem.Services.CampainService;

public class Request
{
    public class CreateCampaignRequest
    {
        public required string Code { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public decimal DiscountValue { get; set; }

        public bool IsPercentage { get; set; }

        public decimal? MinOrderAmount { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        public int Quantity { get; set; }

        public int MaxUsagePerUser { get; set; }

        public bool IsGlobal { get; set; }

        public bool IsNewUserOnly { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }
    }

    public class UpdateCampaignRequest : CreateCampaignRequest
    {
        public Guid Id { get; set; }

        public bool IsActive { get; set; }
    }
}