using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.AffiliateLinkService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(
        AppDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response.CreateAffiliateLinkResponse> CreateAffiliateLink(
        Request.CreateAffiliateLinkRequest request)
    {
        var customerId = GetRequiredGuidClaim("CustomerId");

        var reviewer = await _dbContext.Reviewers
            .FirstOrDefaultAsync(x => x.CustomerId == customerId);

        if (reviewer == null)
            throw new Exception("Reviewer not found");

        var merchant = await _dbContext.Merchants
            .FirstOrDefaultAsync(x => x.Id == request.MerchantId);

        if (merchant == null)
            throw new Exception("Merchant not found");

        var existedAffiliateLink = await _dbContext.AffiliateLinks
            .FirstOrDefaultAsync(x =>
                x.ReviewerId == reviewer.Id &&
                x.MerchantId == merchant.Id);

        if (existedAffiliateLink != null)
        {
            return new Response.CreateAffiliateLinkResponse
            {
                AffiliateLinkId = existedAffiliateLink.Id,
                LinkCode = existedAffiliateLink.LinkCode,
                Url = $"https://u-gem.vercel.app/merchant/{merchant.Id}?ref={existedAffiliateLink.LinkCode}",
                ClickCount = existedAffiliateLink.ClickCount,
                IsActive = existedAffiliateLink.IsActive
            };
        }

        var linkCode =
            $"UGEM-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var affiliateLink = new AffiliateLink
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewer.Id,
            MerchantId = merchant.Id,
            LinkCode = linkCode,
            ClickCount = 0,
            OrderCount = 0,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.AffiliateLinks.Add(affiliateLink);

        await _dbContext.SaveChangesAsync();

        return new Response.CreateAffiliateLinkResponse
        {
            AffiliateLinkId = affiliateLink.Id,
            LinkCode = affiliateLink.LinkCode,
            Url =
                $"https://u-gem.vercel.app/merchant/{affiliateLink.MerchantId}?ref={affiliateLink.LinkCode}",
            ClickCount = affiliateLink.ClickCount,
            IsActive = affiliateLink.IsActive
        };
    }

    public async Task<string> TrackClickAndGetRedirectUrl(string linkCode)
    {
        var affiliateLink = await _dbContext.AffiliateLinks
            .FirstOrDefaultAsync(x => x.LinkCode == linkCode);

        if (affiliateLink == null)
            throw new Exception("Affiliate link not found");

        if (!affiliateLink.IsActive)
            throw new Exception("Affiliate link is inactive");

        // Note: MVP Attribution relies on client-side TTL (7 days). Backend only validates LinkCode existence and merchant match.
        affiliateLink.ClickCount++;
        affiliateLink.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return $"https://u-gem.vercel.app/merchant/{affiliateLink.MerchantId}?ref={affiliateLink.LinkCode}";
    }

    public async Task<Response.ReviewerAffiliateEarningsResponse> GetReviewerAffiliateEarnings()
    {
        var customerId = GetRequiredGuidClaim("CustomerId");

        var reviewer = await _dbContext.Reviewers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CustomerId == customerId);

        if (reviewer == null)
            throw new Exception("Reviewer not found");

        var transactionQuery = _dbContext.ReviewerWalletTransactions
            .AsNoTracking()
            .Where(x => x.ReviewerId == reviewer.Id);

        var totalCommission = await transactionQuery
            .Where(x => x.Type == ReviewerWalletTransactionType.Commission)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        var totalReversal = await transactionQuery
            .Where(x => x.Type == ReviewerWalletTransactionType.Reversal)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        var commissionOrderCount = await transactionQuery
            .Where(x => x.Type == ReviewerWalletTransactionType.Commission && x.Amount > 0)
            .Select(x => x.OrderId)
            .Distinct()
            .CountAsync();

        var affiliateLinks = await _dbContext.AffiliateLinks
            .AsNoTracking()
            .Where(x => x.ReviewerId == reviewer.Id)
            .Select(x => new
            {
                x.ClickCount
            })
            .ToListAsync();

        var recentTransactions = await transactionQuery
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(10)
            .Select(x => new Response.ReviewerAffiliateEarningTransaction
            {
                TransactionId = x.Id,
                OrderId = x.OrderId,
                Amount = x.Amount,
                Type = x.Type,
                EarningsAfter = x.BalanceAfter,
                CreatedAtUtc = x.CreatedAtUtc,
                Reason = x.Reason
            })
            .ToListAsync();

        return new Response.ReviewerAffiliateEarningsResponse
        {
            ReviewerId = reviewer.Id,
            Points = reviewer.Points,
            Rank = reviewer.Rank,
            CurrentEarnings = reviewer.Balance,
            TotalCommission = totalCommission,
            TotalReversal = totalReversal,
            NetEarnings = totalCommission - totalReversal,
            CommissionRate = reviewer.CommissionRate,
            AffiliateLinkCount = affiliateLinks.Count,
            TotalClicks = affiliateLinks.Sum(x => x.ClickCount),
            CommissionOrderCount = commissionOrderCount,
            RecentTransactions = recentTransactions
        };
    }

    private Guid GetRequiredGuidClaim(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == claimType)?.Value;

        if (string.IsNullOrWhiteSpace(value))
            throw new UnauthorizedAccessException($"{claimType} not found");

        return Guid.Parse(value);
    }
}