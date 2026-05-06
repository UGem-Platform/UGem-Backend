using Microsoft.AspNetCore.Http;
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
}