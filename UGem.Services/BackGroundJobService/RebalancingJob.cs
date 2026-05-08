using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using UGem.Repositories;

namespace UGem.Services.BackGroundJobService;

public class RebalancingJob : IJob
{
    private const decimal W_O = 0.4m;
    private const decimal W_R = 0.3m;
    private const decimal W_V = 0.3m;

    private const decimal BaseFee = 0.05m;
    private const decimal GrowthFactor = 0.10m;

    private readonly AppDbContext _dbContext;
    private readonly ILogger<RebalancingJob> _logger;

    public RebalancingJob(AppDbContext dbContext, ILogger<RebalancingJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("RebalancingJob started at {time}", DateTimeOffset.UtcNow);

        var now = DateTimeOffset.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var threeMonthsAgo = monthStart.AddMonths(-3);

        var merchants = await _dbContext.Merchants
            .Where(m => m.IsActive)
            .ToListAsync();

        if (merchants.Count == 0)
        {
            _logger.LogInformation("RebalancingJob completed with no active merchants.");
            return;
        }

        var merchantIds = merchants.Select(m => m.Id).ToList();

        var currentOrderCounts = await GetCompletedOrderCountsAsync(merchantIds, monthStart, null);
        var historicalOrderCounts = await GetCompletedOrderCountsAsync(merchantIds, threeMonthsAgo, monthStart);
        var currentReviewCounts = await GetReviewCountsAsync(merchantIds, monthStart, null);
        var historicalReviewCounts = await GetReviewCountsAsync(merchantIds, threeMonthsAgo, monthStart);
        var currentVisitCounts = await GetVisitCountsAsync(merchantIds, monthStart, null);
        var historicalVisitCounts = await GetVisitCountsAsync(merchantIds, threeMonthsAgo, monthStart);

        var systemAvgOrders = CalculateThreeMonthAverage(historicalOrderCounts, merchants.Count, 100m);
        var systemAvgReviews = CalculateThreeMonthAverage(historicalReviewCounts, merchants.Count, 50m);
        var systemAvgVisits = CalculateThreeMonthAverage(historicalVisitCounts, merchants.Count, 1000m);

        foreach (var merchant in merchants)
        {
            try
            {
                var actualOrders = currentOrderCounts.GetValueOrDefault(merchant.Id);
                var actualReviews = currentReviewCounts.GetValueOrDefault(merchant.Id);
                var actualVisits = currentVisitCounts.GetValueOrDefault(merchant.Id);

                var merchantCreatedMonth = new DateTimeOffset(
                    merchant.CreatedAt.Year,
                    merchant.CreatedAt.Month,
                    1, 0, 0, 0, TimeSpan.Zero);

                var monthsOld = ((monthStart.Year - merchantCreatedMonth.Year) * 12)
                                + (monthStart.Month - merchantCreatedMonth.Month);

                decimal oTarget;
                decimal rTarget;
                decimal vTarget;

                if (monthsOld < 3)
                {
                    oTarget = systemAvgOrders;
                    rTarget = systemAvgReviews;
                    vTarget = systemAvgVisits;
                }
                else
                {
                    oTarget = historicalOrderCounts.GetValueOrDefault(merchant.Id) / 3.0m;
                    rTarget = historicalReviewCounts.GetValueOrDefault(merchant.Id) / 3.0m;
                    vTarget = historicalVisitCounts.GetValueOrDefault(merchant.Id) / 3.0m;

                    if (oTarget == 0) oTarget = systemAvgOrders;
                    if (rTarget == 0) rTarget = systemAvgReviews;
                    if (vTarget == 0) vTarget = systemAvgVisits;
                }

                var O = Math.Min(actualOrders / oTarget, 1.5m);
                var R = Math.Min(actualReviews / rTarget, 1.5m);
                var V = Math.Min(actualVisits / vTarget, 1.5m);
                var SI = (O * W_O + R * W_R + V * W_V) * 100;
                var US = Math.Max(0m, Math.Min(1m, 1 - (SI / 100)));
                var platformFeePercent = (BaseFee + GrowthFactor * (1 - US)) * 100;

                merchant.UnderratedScore = Math.Round(US, 4);
                merchant.PlatformFeePercent = Math.Round(platformFeePercent, 2);
                _logger.LogInformation(
                    "Merchant [{name}]: Orders={actualO}/{targetO}, Reviews={actualR}/{targetR}, Visits={actualV}/{targetV}, SI={si}, US={us}, Fee={fee}%",
                    merchant.Name,
                    actualOrders, Math.Round(oTarget, 0),
                    actualReviews, Math.Round(rTarget, 0),
                    actualVisits, Math.Round(vTarget, 0),
                    Math.Round(SI, 1),
                    Math.Round(US, 4),
                    Math.Round(platformFeePercent, 2));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing merchant {name}: {ex}", merchant.Name, ex.Message);
            }
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("RebalancingJob completed at {time}", DateTimeOffset.UtcNow);
    }

    private async Task<Dictionary<Guid, decimal>> GetCompletedOrderCountsAsync(
        List<Guid> merchantIds,
        DateTimeOffset rangeStart,
        DateTimeOffset? rangeEnd)
    {
        var query = _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od =>
                merchantIds.Contains(od.Food.MerchantId)
                && od.Order.Status == "Completed"
                && od.Order.CreatedAt >= rangeStart);

        if (rangeEnd.HasValue)
        {
            query = query.Where(od => od.Order.CreatedAt < rangeEnd.Value);
        }

        return await query
            .Select(od => new
            {
                MerchantId = od.Food.MerchantId,
                od.OrderId
            })
            .Distinct()
            .GroupBy(x => x.MerchantId)
            .Select(g => new
            {
                MerchantId = g.Key,
                Count = (decimal)g.Count()
            })
            .ToDictionaryAsync(x => x.MerchantId, x => x.Count);
    }

    private async Task<Dictionary<Guid, decimal>> GetReviewCountsAsync(
        List<Guid> merchantIds,
        DateTimeOffset rangeStart,
        DateTimeOffset? rangeEnd)
    {
        var query = _dbContext.Reviews
            .AsNoTracking()
            .Where(r => merchantIds.Contains(r.MerchantId) && r.CreatedAt >= rangeStart);

        if (rangeEnd.HasValue)
        {
            query = query.Where(r => r.CreatedAt < rangeEnd.Value);
        }

        return await query
            .GroupBy(r => r.MerchantId)
            .Select(g => new
            {
                MerchantId = g.Key,
                Count = (decimal)g.Count()
            })
            .ToDictionaryAsync(x => x.MerchantId, x => x.Count);
    }

    private async Task<Dictionary<Guid, decimal>> GetVisitCountsAsync(
        List<Guid> merchantIds,
        DateTimeOffset rangeStart,
        DateTimeOffset? rangeEnd)
    {
        var query = _dbContext.CheckIns
            .AsNoTracking()
            .Where(c => merchantIds.Contains(c.MerchantId) && c.CreatedAt >= rangeStart);

        if (rangeEnd.HasValue)
        {
            query = query.Where(c => c.CreatedAt < rangeEnd.Value);
        }

        return await query
            .GroupBy(c => c.MerchantId)
            .Select(g => new
            {
                MerchantId = g.Key,
                Count = (decimal)g.Count()
            })
            .ToDictionaryAsync(x => x.MerchantId, x => x.Count);
    }

    private static decimal CalculateThreeMonthAverage(
        IReadOnlyDictionary<Guid, decimal> counts,
        int merchantCount,
        decimal fallback)
    {
        if (merchantCount == 0)
        {
            return fallback;
        }

        var average = counts.Values.Sum() / merchantCount / 3.0m;
        return average > 0 ? average : fallback;
    }
}
