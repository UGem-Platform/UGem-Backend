
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.StaffService;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
   
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task ApproveApplication(Request.ApproveReviewerApplicationRequest request)
    {
        var app = await _dbContext.ReviewerApplications
            .FirstOrDefaultAsync(x => x.Id == request.ApplicationId);

        if (app == null)
            throw new KeyNotFoundException("Application not found");

        if (app.Status != "Pending")
            throw new InvalidOperationException("Only pending application can be approved");
        app.Status = "Accept";
        app.UpdatedAt = DateTimeOffset.UtcNow;
        var existReviewer = await _dbContext.Reviewers.AnyAsync(x => x.CustomerId == app.CustomerId);
        if (existReviewer)
        {
            throw new InvalidOperationException("Application already approved");
        }

        var reviewer = new Reviewer
        {
            CustomerId = app.CustomerId,
            Points = 0,
            Rank = "Bronze",
            CommissionRate = 0.05m

        };
        _dbContext.Reviewers.Add(reviewer);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RejectApplication(Request.RejectReviewerApplicationRequest request)
    {
        var app = await _dbContext.ReviewerApplications
            .FirstOrDefaultAsync(x => x.Id == request.ApplicationId);

        if (app == null)
            throw new KeyNotFoundException("Application not found");

        if (app.Status != "Pending")
            throw new InvalidOperationException("Only pending application can be rejected");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException("Rejection reason is required");

        app.Status = "Rejected";
        app.RejectionReason = request.Reason;
        app.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<Base.Response.PageResult<Response.ReviewerApplicationResponse>> GetReviewerApplications(string? searchTerm, int pageSize, int pageIndex)
    {
        var query = _dbContext.ReviewerApplications.Where(x => true);
        if (searchTerm != null)
        {
            query = query.Where(x =>
                x.Motivation.Contains(searchTerm) ||
                (x.Experience != null && x.Experience.Contains(searchTerm)) ||
                (x.FacebookUrl != null && x.FacebookUrl.Contains(searchTerm)) ||
                (x.TiktokUrl != null && x.TiktokUrl.Contains(searchTerm)) ||
                (x.YoutubeUrl != null && x.YoutubeUrl.Contains(searchTerm))
            );
        }
        query = query.OrderByDescending(x => x.CreatedAt);
        query = query.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize);
        var selectQuery = query.Select(x => new Response.ReviewerApplicationResponse()
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
        });
        var listResult = await selectQuery.ToListAsync();
        var totalItems = listResult.Count();
        var result = new Base.Response.PageResult<Response.ReviewerApplicationResponse>()
        {
            Items = listResult,
            PageSize = pageSize,
            PageIndex = pageIndex,
            TotalItems = totalItems
        };
        return result;

    }
}