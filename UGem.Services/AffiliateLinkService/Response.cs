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

    public class ReviewerAffiliateEarningsResponse
    {
        public Guid ReviewerId { get; set; }

        public int Points { get; set; }

        public required string Rank { get; set; }

        public decimal CurrentEarnings { get; set; }

        public decimal TotalCommission { get; set; }

        public decimal TotalReversal { get; set; }

        public decimal NetEarnings { get; set; }

        public decimal CommissionRate { get; set; }

        public int AffiliateLinkCount { get; set; }

        public int TotalClicks { get; set; }

        public int CommissionOrderCount { get; set; }

        public List<ReviewerAffiliateEarningTransaction> RecentTransactions { get; set; } = new();
    }

    public class ReviewerAffiliateEarningTransaction
    {
        public Guid TransactionId { get; set; }

        public Guid OrderId { get; set; }

        public decimal Amount { get; set; }

        public required string Type { get; set; }

        public decimal EarningsAfter { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; }

        public string? Reason { get; set; }
    }
}