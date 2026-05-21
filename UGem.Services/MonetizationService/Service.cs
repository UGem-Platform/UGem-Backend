using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.MonetizationService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandlePaymentSuccess(Guid orderId)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
            .Include(o => o.AffiliateLink)
                .ThenInclude(al => al!.Reviewer)
                    .ThenInclude(r => r.Customer)
                        .ThenInclude(c => c.User)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Food)
                    .ThenInclude(f => f.Merchant)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new Exception($"Order {orderId} not found");
        }

        if (order.MonetizationProcessedAtUtc != null)
        {
            return;
        }

        if (order.FinalPrice <= 0)
        {
            return;
        }

        var merchantIds = order.OrderDetails.Select(od => od.Food.MerchantId).Distinct().ToList();
        if (merchantIds.Count > 1)
        {
            // Log error and possibly throw or handle
            throw new Exception("Order contains items from multiple merchants. This is not supported for monetization.");
        }

        if (merchantIds.Count == 0)
        {
             throw new Exception("Order contains no items.");
        }

        var merchant = order.OrderDetails.First().Food.Merchant;

        decimal reviewerFee = 0;
        Reviewer? targetReviewer = null;
        string? skipReason = null;

        if (order.AffiliateLink != null)
        {
            var affiliateLink = order.AffiliateLink;
            targetReviewer = affiliateLink.Reviewer;

            if (targetReviewer == null)
            {
                skipReason = "Affiliate link has no associated reviewer.";
            }
            else
            {
                bool isSelfReferral = order.Customer != null && targetReviewer.Customer != null && order.Customer.UserId == targetReviewer.Customer.UserId;
                bool isMerchantSelfPurchase = order.Customer != null && order.Customer.UserId == merchant.UserId;

                if (!affiliateLink.IsActive)
                {
                    skipReason = "Affiliate link is inactive.";
                }
                else if (isSelfReferral)
                {
                    skipReason = "Self-referral detected.";
                }
                else if (isMerchantSelfPurchase)
                {
                    skipReason = "Merchant self-purchase detected.";
                }
                else
                {
                    var commissionRate = GetReviewerCommissionRate(targetReviewer);
                    reviewerFee = order.FinalPrice * commissionRate;
                }
            }

            if (targetReviewer != null)
            {
                targetReviewer.Balance += reviewerFee;

                var transaction = new ReviewerWalletTransaction
                {
                    Id = Guid.NewGuid(),
                    ReviewerId = targetReviewer.Id,
                    OrderId = order.Id,
                    Amount = reviewerFee,
                    Type = ReviewerWalletTransactionType.Commission,
                    BalanceAfter = targetReviewer.Balance,
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    Reason = skipReason ?? $"Commission from order {order.Name}"
                };

                _dbContext.ReviewerWalletTransactions.Add(transaction);
            }
        }

        order.ReviewerFee = reviewerFee;
        order.PlatformFee = order.FinalPrice * (merchant.PlatformFeePercent / 100);
        order.MonetizationProcessedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    private static decimal GetReviewerCommissionRate(Reviewer reviewer)
    {
        if (reviewer.CommissionRate > 0)
        {
            return reviewer.CommissionRate > 1
                ? reviewer.CommissionRate / 100
                : reviewer.CommissionRate;
        }

        if (reviewer.Points >= 100) return 0.02m;
        if (reviewer.Points >= 50) return 0.01m;
        if (reviewer.Points >= 20) return 0.005m;

        return 0;
    }

    public async Task ProcessCompletedOrdersMissingMonetization(Guid? merchantId = null, Guid? reviewerId = null)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .Where(o =>
                o.Status == "Completed" &&
                o.FinalPrice > 0 &&
                o.MonetizationProcessedAtUtc == null);

        if (merchantId.HasValue)
        {
            query = query.Where(o => o.OrderDetails.Any(od => od.Food.MerchantId == merchantId.Value));
        }

        if (reviewerId.HasValue)
        {
            query = query.Where(o => o.AffiliateLink != null && o.AffiliateLink.ReviewerId == reviewerId.Value);
        }

        var orderIds = await query
            .Select(o => o.Id)
            .ToListAsync();

        foreach (var orderId in orderIds)
        {
            await HandlePaymentSuccess(orderId);
        }
    }

    public async Task ReprocessCompletedOrder(Guid orderId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new Exception($"Order {orderId} not found");
        }

        if (order.Status != "Completed")
        {
            throw new InvalidOperationException("Only completed orders can be reprocessed for monetization.");
        }

        var commissionTransactions = await _dbContext.ReviewerWalletTransactions
            .Where(t => t.OrderId == orderId && t.Type == ReviewerWalletTransactionType.Commission)
            .ToListAsync();

        if (commissionTransactions.Count > 0)
        {
            var reviewerIds = commissionTransactions
                .Select(t => t.ReviewerId)
                .Distinct()
                .ToList();

            var reviewers = await _dbContext.Reviewers
                .Where(r => reviewerIds.Contains(r.Id))
                .ToListAsync();

            foreach (var reviewer in reviewers)
            {
                var amount = commissionTransactions
                    .Where(t => t.ReviewerId == reviewer.Id)
                    .Sum(t => t.Amount);

                reviewer.Balance -= amount;
            }

            _dbContext.ReviewerWalletTransactions.RemoveRange(commissionTransactions);
        }

        order.ReviewerFee = 0;
        order.PlatformFee = 0;
        order.MonetizationProcessedAtUtc = null;

        await _dbContext.SaveChangesAsync();
        await HandlePaymentSuccess(orderId);
        await transaction.CommitAsync();
    }

    public async Task HandleRefund(Guid orderId)
    {
        var order = await _dbContext.Orders
            .Include(o => o.AffiliateLink)
                .ThenInclude(al => al!.Reviewer)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return;

        var commissionTransaction = await _dbContext.ReviewerWalletTransactions
            .FirstOrDefaultAsync(t => t.OrderId == orderId && t.Type == ReviewerWalletTransactionType.Commission);

        if (commissionTransaction == null) return;

        var alreadyReversed = await _dbContext.ReviewerWalletTransactions
            .AnyAsync(t => t.OrderId == orderId && t.Type == ReviewerWalletTransactionType.Reversal);

        if (alreadyReversed) return;

        var reviewer = await _dbContext.Reviewers.FindAsync(commissionTransaction.ReviewerId);
        if (reviewer != null)
        {
            reviewer.Balance -= commissionTransaction.Amount;

            var reversalTransaction = new ReviewerWalletTransaction
            {
                Id = Guid.NewGuid(),
                ReviewerId = reviewer.Id,
                OrderId = orderId,
                Amount = commissionTransaction.Amount,
                Type = ReviewerWalletTransactionType.Reversal,
                BalanceAfter = reviewer.Balance,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                Reason = $"Reversal of commission for refunded order {orderId}"
            };

            _dbContext.ReviewerWalletTransactions.Add(reversalTransaction);
        }

        await _dbContext.SaveChangesAsync();
    }
}
