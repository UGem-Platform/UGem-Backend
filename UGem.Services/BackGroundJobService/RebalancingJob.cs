using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using UGem.Repositories;

namespace UGem.Services.BackGroundJobService;

public class RebalancingJob : IJob
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RebalancingJob> _logger;

   
    private const decimal W_O = 0.4m; 
    private const decimal W_R = 0.3m; 
    private const decimal W_V = 0.3m; 

  
    private const decimal BaseFee = 0.05m;     
    private const decimal GrowthFactor = 0.10m; 

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
        var systemAvgOrders = 100m; 
        var systemAvgReviews = 50m;
        var systemAvgVisits = 1000m;

        try
        {
            var allMerchantOrderCounts = await _dbContext.Merchants
                .Where(m => m.IsActive)
                .Select(m => _dbContext.Orders
                    .Count(o => o.CreatedAt >= threeMonthsAgo
                        && o.CreatedAt < monthStart
                        && o.Status == "Completed"
                        && o.OrderDetails.Any(od => od.Food.MerchantId == m.Id)))
                .ToListAsync();

            if (allMerchantOrderCounts.Any())
                systemAvgOrders = (decimal)allMerchantOrderCounts.Average() / 3.0m;

            var allMerchantReviewCounts = await _dbContext.Merchants
                .Where(m => m.IsActive)
                .Select(m => _dbContext.Reviews
                    .Count(r => r.MerchantId == m.Id
                        && r.CreatedAt >= threeMonthsAgo
                        && r.CreatedAt < monthStart))
                .ToListAsync();

            if (allMerchantReviewCounts.Any())
                systemAvgReviews = (decimal)allMerchantReviewCounts.Average() / 3.0m;

            var allMerchantVisitCounts = await _dbContext.Merchants
                .Where(m => m.IsActive)
                .Select(m => _dbContext.CheckIns
                    .Count(c => c.MerchantId == m.Id
                        && c.CreatedAt >= threeMonthsAgo
                        && c.CreatedAt < monthStart))
                .ToListAsync();

            if (allMerchantVisitCounts.Any())
                systemAvgVisits = (decimal)allMerchantVisitCounts.Average() / 3.0m;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not calculate system averages, using defaults. Error: {ex}", ex.Message);
        }

        
        foreach (var merchant in merchants)
        {
            try
            {
                
                var actualOrders = await _dbContext.Orders
                    .CountAsync(o => o.CreatedAt >= monthStart
                        && o.Status == "Completed"
                        && o.OrderDetails.Any(od => od.Food.MerchantId == merchant.Id));
                var actualReviews = await _dbContext.Reviews
                    .CountAsync(r => r.MerchantId == merchant.Id
                        && r.CreatedAt >= monthStart);
                var actualVisits = await _dbContext.CheckIns
                    .CountAsync(c => c.MerchantId == merchant.Id
                        && c.CreatedAt >= monthStart);
                var merchantCreatedMonth = new DateTimeOffset(
                    merchant.CreatedAt.Year, 
                    merchant.CreatedAt.Month, 
                    1, 0, 0, 0, TimeSpan.Zero);

                var monthsOld = ((monthStart.Year - merchantCreatedMonth.Year) * 12) 
                                + (monthStart.Month - merchantCreatedMonth.Month);

                decimal oTarget, rTarget, vTarget;

                if (monthsOld < 3) 
                {
                    
                    oTarget = systemAvgOrders > 0 ? systemAvgOrders : 100;
                    rTarget = systemAvgReviews > 0 ? systemAvgReviews : 50;
                    vTarget = systemAvgVisits > 0 ? systemAvgVisits : 1000;
                }
                else
                {
                    var historyOrders = await _dbContext.Orders
                        .CountAsync(o => o.CreatedAt >= threeMonthsAgo
                            && o.CreatedAt < monthStart
                            && o.Status == "Completed"
                            && o.OrderDetails.Any(od => od.Food.MerchantId == merchant.Id));

                    var historyReviews = await _dbContext.Reviews
                        .CountAsync(r => r.MerchantId == merchant.Id
                            && r.CreatedAt >= threeMonthsAgo
                            && r.CreatedAt < monthStart);

                    var historyVisits = await _dbContext.CheckIns
                        .CountAsync(c => c.MerchantId == merchant.Id
                            && c.CreatedAt >= threeMonthsAgo
                            && c.CreatedAt < monthStart);
                    oTarget = historyOrders / 3.0m;
                    rTarget = historyReviews / 3.0m;
                    vTarget = historyVisits / 3.0m;
                    if (oTarget == 0) oTarget = systemAvgOrders > 0 ? systemAvgOrders : 100;
                    if (rTarget == 0) rTarget = systemAvgReviews > 0 ? systemAvgReviews : 50;
                    if (vTarget == 0) vTarget = systemAvgVisits > 0 ? systemAvgVisits : 1000;
                }
                var O = Math.Min((decimal)actualOrders / oTarget, 1.5m);
                var R = Math.Min((decimal)actualReviews / rTarget, 1.5m);
                var V = Math.Min((decimal)actualVisits / vTarget, 1.5m);
                var SI = (O * W_O + R * W_R + V * W_V) * 100;
                var US = Math.Max(0m, Math.Min(1m, 1 - (SI / 100)));
                var platformFeePercent = (BaseFee + GrowthFactor * (1 - US)) * 100;
                
                merchant.UnderratedScore = Math.Round(US, 4);
                merchant.PlatformFeePercent = Math.Round(platformFeePercent, 2);
                _logger.LogInformation(
                    "Merchant [{name}]: " +
                    "Orders={actualO}/{targetO}, " +
                    "Reviews={actualR}/{targetR}, " +
                    "Visits={actualV}/{targetV}, " +
                    "SI={si}, US={us}, Fee={fee}%",
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
}