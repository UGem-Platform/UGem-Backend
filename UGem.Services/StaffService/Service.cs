using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.StaffService;

public class Service : IService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ApproveApplication(Request.ApproveReviewerApplicationRequest request)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var app = await _dbContext.ReviewerApplications.Include(reviewerApplication => reviewerApplication.Customer)
            .ThenInclude(customer => customer.User)
            .FirstOrDefaultAsync(x => x.Id == request.ApplicationId);

        if (app == null)
        {
            throw new KeyNotFoundException("Application not found");
        }

        if (app.Status != "Pending")
        {
            throw new InvalidOperationException("Only pending application can be approved");
        }

        var existReviewer = await _dbContext.Reviewers.AnyAsync(x => x.CustomerId == app.CustomerId);
        if (existReviewer)
        {
            throw new InvalidOperationException("Application already approved");
        }

        app.Status = "Accept";
        app.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.Reviewers.Add(new Reviewer
        {
            CustomerId = app.CustomerId,
            Points = 0,
            Rank = "Bronze",
            CommissionRate = 0.05m
        });
        _dbContext.Notifications.Add(new Notification
        {
            UserId = app.Customer.UserId,
            Title = "Reviewer Application Approved",
            Message = $"Congratulations {app.Customer.User.FullName}! You have officially become a Reviewer on UGem.",
            Type = "ReviewerApplication",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task RejectApplication(Request.RejectReviewerApplicationRequest request)
    {
        var app = await _dbContext.ReviewerApplications.Include(reviewerApplication => reviewerApplication.Customer)
            .ThenInclude(customer => customer.User)
            .FirstOrDefaultAsync(x => x.Id == request.ApplicationId);

        if (app == null)
        {
            throw new KeyNotFoundException("Application not found");
        }

        if (app.Status != "Pending")
        {
            throw new InvalidOperationException("Only pending application can be rejected");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ArgumentException("Rejection reason is required");
        }

        app.Status = "Rejected";
        app.RejectionReason = request.Reason;
        app.UpdatedAt = DateTimeOffset.UtcNow;
        _dbContext.Notifications.Add(new Notification
        {
            UserId = app.Customer.UserId,
            Title = "Reviewer Application Rejected",
            Message = $"Sorry {app.Customer.User.FullName}, your Reviewer application has been rejected. Reason: {request.Reason}",
            Type = "ReviewerApplication",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Base.Response.PageResult<Response.ReviewerApplicationResponse>> GetReviewerApplications(string? searchTerm, int pageSize, int pageIndex)
    {
        var (normalizedPageIndex, normalizedPageSize) = NormalizePagination(pageIndex, pageSize);
        var query = _dbContext.ReviewerApplications.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x =>
                x.Motivation.Contains(searchTerm) ||
                (x.Experience != null && x.Experience.Contains(searchTerm)) ||
                (x.FacebookUrl != null && x.FacebookUrl.Contains(searchTerm)) ||
                (x.TiktokUrl != null && x.TiktokUrl.Contains(searchTerm)) ||
                (x.YoutubeUrl != null && x.YoutubeUrl.Contains(searchTerm)));
        }

        var totalItems = await query.CountAsync();

        var listResult = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((normalizedPageIndex - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => new Response.ReviewerApplicationResponse
            {
                Id = x.Id,
                Status = x.Status,
                Motivation = x.Motivation,
                Experience = x.Experience,
                FacebookUrl = x.FacebookUrl,
                TiktokUrl = x.TiktokUrl,
                YoutubeUrl = x.YoutubeUrl,
                OtherSocialUrl = x.OtherSocialUrl,
                RejectionReason = x.RejectionReason,
                CustomerId = x.CustomerId,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return new Base.Response.PageResult<Response.ReviewerApplicationResponse>
        {
            Items = listResult,
            PageSize = normalizedPageSize,
            PageIndex = normalizedPageIndex,
            TotalItems = totalItems
        };
    }

    private static (int PageIndex, int PageSize) NormalizePagination(int pageIndex, int pageSize)
    {
        var normalizedPageIndex = pageIndex <= 0 ? 1 : pageIndex;
        var normalizedPageSize = pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
        return (normalizedPageIndex, normalizedPageSize);
    }
}
