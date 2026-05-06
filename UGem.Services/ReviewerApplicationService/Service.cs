using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.ReviewerApplicationService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task CreateReviewerApplication(Request.ReviewerApplicationRequest request)
    {
        var customerId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;
        
        var customerIdGuid = Guid.Parse(customerId!);

        if (string.IsNullOrEmpty(customerId))
        {
            throw new KeyNotFoundException("CustomerId not found");
        }
        
        if (string.IsNullOrWhiteSpace(request.FacebookUrl) &&
              string.IsNullOrWhiteSpace(request.TiktokUrl) &&
              string.IsNullOrWhiteSpace(request.YoutubeUrl) &&
              string.IsNullOrWhiteSpace(request.OtherSocialUrl))
        {
            throw new Exception("At least one social link is required");
        }
        
        var reviewerApplication = new ReviewerApplication()
        {

            CustomerId = customerIdGuid,
            Motivation = request.Motivation,
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow,
            Experience = request.Experience,
            FacebookUrl = request.FacebookUrl,
            TiktokUrl = request.TiktokUrl,
            YoutubeUrl = request.YoutubeUrl,
            OtherSocialUrl = request.OtherSocialUrl,
        };
        _dbContext.Add(reviewerApplication);
        await _dbContext.SaveChangesAsync();
    }
    public async Task UpdateReviewerApplication(Request.UpdateReviewerApplicationRequest request)
    {
        var customerId = _httpContext.HttpContext.User.Claims
            .FirstOrDefault(x => x.Type == "CustomerId")?.Value;

        if (string.IsNullOrEmpty(customerId))
            throw new UnauthorizedAccessException("CustomerId not found");

        var customerIdGuid = Guid.Parse(customerId);

        var application = await _dbContext.ReviewerApplications
            .FirstOrDefaultAsync(x => x.Id == request.reviewerApplicationId);

        if (application == null)
            throw new KeyNotFoundException("Application not found");

        if (application.CustomerId != customerIdGuid)
            throw new UnauthorizedAccessException("You cannot edit this application");

        if (application.Status != "Pending")
            throw new InvalidOperationException("Only pending application can be edited");
        
        if (request.Motivation != null)
            application.Motivation = request.Motivation;

        if (request.Experience != null)
            application.Experience = request.Experience;

        if (request.FacebookUrl != null)
            application.FacebookUrl = request.FacebookUrl;

        if (request.TiktokUrl != null)
            application.TiktokUrl = request.TiktokUrl;

        if (request.YoutubeUrl != null)
            application.YoutubeUrl = request.YoutubeUrl;

        if (request.OtherSocialUrl != null)
            application.OtherSocialUrl = request.OtherSocialUrl;

        if (string.IsNullOrWhiteSpace(application.FacebookUrl) &&
            string.IsNullOrWhiteSpace(application.TiktokUrl) &&
            string.IsNullOrWhiteSpace(application.YoutubeUrl) &&
            string.IsNullOrWhiteSpace(application.OtherSocialUrl))
        {
            throw new ArgumentException("At least one social link is required");
        }

        application.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
}