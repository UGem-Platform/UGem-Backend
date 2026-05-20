namespace UGem.Services.CampainService;

public class Response
{
    public class CampaignResponse
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public decimal DiscountValue { get; set; }

        public bool IsPercentage { get; set; }

        public decimal? MinOrderAmount { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        public int Quantity { get; set; }

        public int UsedCount { get; set; }

        public int MaxUsagePerUser { get; set; }

        public bool IsGlobal { get; set; }

        public bool IsNewUserOnly { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public Guid? MerchantId { get; set; }
    }
    public class ApplyCampaignResponse
    {
        public Guid CampaignId { get; set; }

        public string Code { get; set; }

        public string Title { get; set; }

        public bool IsPercentage { get; set; }

        public decimal DiscountValue { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        public decimal MinOrderAmount { get; set; }

        public string Message { get; set; }
    }
}