using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using UGem.Repositories;
using UGem.Repositories.Entity;

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
        var threeDaysAgo = now.AddDays(-3);
        
        var merchants = await _dbContext.Merchants
            .Where(m => m.IsActive)
            .ToListAsync();

        if (!merchants.Any())
        {
            _logger.LogInformation("No active merchants found. Job completed.");
            return;
        }
        
        var systemAvgOrders = 100m;
        var systemAvgReviews = 50m;
        var systemAvgVisits = 1000m;

        try
        {
            var allOrderCounts = await _dbContext.Merchants
                .Where(m => m.IsActive)
                .Select(m => _dbContext.Orders
                    .Count(o => o.CreatedAt >= threeDaysAgo
                        && o.CreatedAt < monthStart
                        && o.Status == "Completed"
                        && o.OrderDetails.Any(od => od.Food.MerchantId == m.Id)))
                .ToListAsync();

            if (allOrderCounts.Any(x => x > 0))
                systemAvgOrders = (decimal)allOrderCounts.Average() / 3.0m;

            var allReviewCounts = await _dbContext.Merchants
                .Where(m => m.IsActive)
                .Select(m => _dbContext.Reviews
                    .Count(r => r.MerchantId == m.Id
                        && r.CreatedAt >= threeDaysAgo
                        && r.CreatedAt < monthStart))
                .ToListAsync();

            if (allReviewCounts.Any(x => x > 0))
                systemAvgReviews = (decimal)allReviewCounts.Average() / 3.0m;

            var allVisitCounts = await _dbContext.Merchants
                .Where(m => m.IsActive)
                .Select(m => _dbContext.CheckIns
                    .Count(c => c.MerchantId == m.Id
                        && c.CreatedAt >= threeDaysAgo
                        && c.CreatedAt < monthStart))
                .ToListAsync();

            if (allVisitCounts.Any(x => x > 0))
                systemAvgVisits = (decimal)allVisitCounts.Average() / 3.0m;
        }
        catch (Exception ex) when (ex is DbUpdateException || ex is InvalidOperationException)
        {
            _logger.LogWarning("Could not calculate system averages, using defaults. Error: {ex}", ex.Message);
        }
        
        var merchantSiMap = new Dictionary<Guid, decimal>();

        foreach (var merchant in merchants)
        {
            try
            {
                var actualOrders = await _dbContext.Orders
                    .CountAsync(o => o.CreatedAt >= threeDaysAgo
                        && o.Status == "Completed"
                        && o.OrderDetails.Any(od => od.Food.MerchantId == merchant.Id));

                var actualReviews = await _dbContext.Reviews
                    .CountAsync(r => r.MerchantId == merchant.Id
                        && r.CreatedAt >= threeDaysAgo);

                var actualVisits = await _dbContext.CheckIns
                    .CountAsync(c => c.MerchantId == merchant.Id
                        && c.CreatedAt >= threeDaysAgo);
                var daysOld = (now - merchant.CreatedAt).Days;

                decimal oTarget, rTarget, vTarget;

                if (daysOld < 3)
                {
                    oTarget = systemAvgOrders;
                    rTarget = systemAvgReviews;
                    vTarget = systemAvgVisits;
                }
                else
                {
                    var historyOrders = await _dbContext.Orders
                        .CountAsync(o => o.CreatedAt >= threeDaysAgo
                            && o.CreatedAt < monthStart
                            && o.Status == "Completed"
                            && o.OrderDetails.Any(od => od.Food.MerchantId == merchant.Id));

                    var historyReviews = await _dbContext.Reviews
                        .CountAsync(r => r.MerchantId == merchant.Id
                            && r.CreatedAt >= threeDaysAgo
                            && r.CreatedAt < monthStart);

                    var historyVisits = await _dbContext.CheckIns
                        .CountAsync(c => c.MerchantId == merchant.Id
                            && c.CreatedAt >= threeDaysAgo
                            && c.CreatedAt < monthStart);

                    oTarget = historyOrders / 3.0m;
                    rTarget = historyReviews / 3.0m;
                    vTarget = historyVisits / 3.0m;
                    if (oTarget == 0) oTarget = systemAvgOrders;
                    if (rTarget == 0) rTarget = systemAvgReviews;
                    if (vTarget == 0) vTarget = systemAvgVisits;
                }

                
                var O = Math.Min((decimal)actualOrders / oTarget, 1.5m);
                var R = Math.Min((decimal)actualReviews / rTarget, 1.5m);
                var V = Math.Min((decimal)actualVisits / vTarget, 1.5m);

             
                var SI = (O * W_O + R * W_R + V * W_V) * 100;

                merchantSiMap[merchant.Id] = SI;

                _logger.LogInformation(
                    "Merchant [{name}]: Orders={o}/{ot}, Reviews={r}/{rt}, Visits={v}/{vt}, SI={si}",
                    merchant.Name,
                    actualOrders, Math.Round(oTarget, 0),
                    actualReviews, Math.Round(rTarget, 0),
                    actualVisits, Math.Round(vTarget, 0),
                    Math.Round(SI, 1));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error calculating SI for merchant {name}: {ex}", merchant.Name, ex.Message);
                merchantSiMap[merchant.Id] = 0;
            }
        }
        var SI_min = merchantSiMap.Values.Min();
        var SI_max = merchantSiMap.Values.Max();

        _logger.LogInformation("SI_min={min}, SI_max={max}", Math.Round(SI_min, 1), Math.Round(SI_max, 1));
        foreach (var merchant in merchants)
        {
            try
            {
                var SI = merchantSiMap[merchant.Id];
                decimal US;
                if (SI_max == SI_min)
                {

                    US = 0.5m;
                }
                else
                {
                    US = 1 - ((SI - SI_min) / (SI_max - SI_min));
                    US = Math.Max(0m, Math.Min(1m, US));
                }

                var platformFeePercent = (BaseFee + GrowthFactor * (1 - US)) * 100;
                merchant.UnderratedScore = Math.Round(US, 4);
                merchant.PlatformFeePercent = Math.Round(platformFeePercent, 2);

                _logger.LogInformation(
                    "Merchant [{name}]: SI={si}, US={us}, Fee={fee}%",
                    merchant.Name,
                    Math.Round(SI, 1),
                    Math.Round(US, 4),
                    Math.Round(platformFeePercent, 2));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Missing SI value while calculating US for merchant {name}", merchant.Name);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while calculating US for merchant {name}", merchant.Name);
            }
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("RebalancingJob completed at {time}", DateTimeOffset.UtcNow);
    }
}