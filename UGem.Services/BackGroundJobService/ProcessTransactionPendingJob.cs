using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using Quartz;
namespace UGem.Services.BackGroundJobService;

public class ProcessTransactionPendingJob : IJob
{
    private const string PendingStatus = "Pending";
    private const string ExpiredStatus = "Expired";
    private static readonly TimeSpan DefaultPendingTimeout = TimeSpan.FromMinutes(15);

    private readonly AppDbContext _dbContext;
    
    private readonly ILogger _logger;

    public ProcessTransactionPendingJob(
        AppDbContext dbContext,
        ILogger<ProcessTransactionPendingJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var pendingTimeoutMinutes = (int)DefaultPendingTimeout.TotalMinutes;

        var now = DateTimeOffset.UtcNow;
        var threshold = now.AddMinutes(-pendingTimeoutMinutes);

        var expiredPendingOrders = await _dbContext.Orders
            .Where(o => o.Status == PendingStatus && o.CreatedAt <= threshold)
            .ToListAsync(context.CancellationToken);

        if (expiredPendingOrders.Count == 0)
        {
            _logger.LogInformation("ProcessTransactionPendingJob completed: no expired pending orders found.");
            return;
        }

        foreach (var order in expiredPendingOrders)
        {
            order.Status = ExpiredStatus;
            order.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "ProcessTransactionPendingJob completed: expired {ExpiredCount} pending orders older than {PendingTimeoutMinutes} minutes.",
            expiredPendingOrders.Count,
            pendingTimeoutMinutes);
    }
}