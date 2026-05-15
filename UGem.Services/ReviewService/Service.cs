using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.ReviewService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public Task<Base.Response.PageResult<Response.GetReviewsResponse>> GetReviews(Guid? merchantId, int pageSize, int pageIndex)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Response.ReviewsByIdMerchantResponse>> GetReviewByMerchantId(
        Request.GetReviewByMerchantIdRequest request)
    {
        var isExists = await _dbContext.Merchants
            .AnyAsync(x => x.Id == request.MerchantId);

        if (!isExists)
        {
            throw new KeyNotFoundException($"Merchant with id {request.MerchantId} not found");
        }

        return await _dbContext.Reviews
            .AsNoTracking()
            .Where(x => x.MerchantId == request.MerchantId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new Response.ReviewsByIdMerchantResponse
            {
                Id = x.Id,
                MerchantId = x.MerchantId,
                OrderId = x.OrderId,
                Content = x.Content,
                Rating = x.Rating,
                ImageUrl = x.ImageUrl,
                CreatedAt = x.CreatedAt,
                CustomerName = x.Order.Customer.User.FullName,
                CustomerAvatarUrl = x.Order.Customer.User.AvatarUrl
            })
            .ToListAsync();
    }

    public async Task<List<Response.ReviewDetailResponse>> GetReviewDetailsByMerchant(
        Request.GetReviewDetailsByMerchantRequest request)
    {
        var isExists = await _dbContext.Reviews
            .AnyAsync(x => x.Id == request.ReviewId);

        if (!isExists)
        {
            throw new KeyNotFoundException($"Review with id {request.ReviewId} not found");
        }

        return await _dbContext.ReviewDetails
            .AsNoTracking()
            .Where(x => x.ReviewId == request.ReviewId)
            .Select(x => new Response.ReviewDetailResponse
            {
                Id = x.Id,
                ReviewId = x.ReviewId,
                OrderDetailId = x.OrderDetailId,
                DetailContent = x.DetailContent,
                Rating = x.Rating,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task ReviewMerchant(Request.ReviewByMerchantIdRequest request)
    {
        var customerIdGuid = GetRequiredGuidClaim("CustomerId");

        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.CustomerId == customerIdGuid
                                                                     && x.Id == request.OrderId);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {request.OrderId} not found");
        }

        if (order.Status != "Completed")
        {
            throw new InvalidOperationException("Order must be completed before review.");
        }

        var merchant = await _dbContext.Merchants.FirstOrDefaultAsync(x => x.Id == request.MerchantId);
        if (merchant == null)
        {
            throw new KeyNotFoundException($"Merchant with id {request.MerchantId} not found");
        }

        var merchantMatchesOrder = await _dbContext.OrderDetails
            .AnyAsync(x => x.OrderId == request.OrderId && x.Food.MerchantId == request.MerchantId);

        if (!merchantMatchesOrder)
        {
            throw new InvalidOperationException("Merchant does not match the order.");
        }

        if (request.Rating is < 1 or > 5)
        {
            throw new InvalidOperationException("Rating must be between 1 and 5.");
        }

        var existedReview = await _dbContext.Reviews
            .AnyAsync(x => x.OrderId == request.OrderId);

        if (existedReview)
        {
            throw new InvalidOperationException("Order has been reviewed.");
        }

        var requestedDetails = request.ReviewDetails?.ToList() ?? [];
        ValidateRequestedRatings(requestedDetails.Select(detail => detail.Rating));

        var validOrderDetailIds = requestedDetails.Count == 0
            ? new HashSet<Guid>()
            : (await _dbContext.OrderDetails
                .AsNoTracking()
                .Where(x => x.OrderId == request.OrderId)
                .Select(x => x.Id)
                .ToListAsync()).ToHashSet();

        foreach (var detail in requestedDetails)
        {
            if (!validOrderDetailIds.Contains(detail.OrderDetailId))
            {
                throw new InvalidOperationException($"OrderDetailId {detail.OrderDetailId} unValid");
            }
        }

        var existingRatings = await _dbContext.Reviews
            .AsNoTracking()
            .Where(x => x.MerchantId == request.MerchantId)
            .Select(x => x.Rating)
            .ToListAsync();

        var reviewId = Guid.NewGuid();
        _dbContext.Reviews.Add(new Review
        {
            Id = reviewId,
            MerchantId = request.MerchantId,
            OrderId = request.OrderId,
            Content = request.Content,
            Rating = request.Rating,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        if (requestedDetails.Count > 0)
        {
            _dbContext.ReviewDetails.AddRange(requestedDetails.Select(detail => new ReviewDetail
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                OrderDetailId = detail.OrderDetailId,
                DetailContent = detail.DetailContent,
                Rating = detail.Rating,
            }));
        }

        merchant.Rating = CalculateAverageRating(existingRatings, request.Rating);
        merchant.UpdatedAt = DateTimeOffset.UtcNow;
        _dbContext.Notifications.Add(new Notification
        {
            UserId = merchant.UserId,
            Message = "Your merchant has a new receive review",
            Title = "New review received",
            Type = "Review",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateReviewMerchant(Request.UpdateReviewByMerchantIdRequest request)
    {
        var customerIdGuid = GetRequiredGuidClaim("CustomerId");

        var review = await _dbContext.Reviews
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == request.ReviewId);

        if (review == null)
        {
            throw new KeyNotFoundException($"Review with id {request.ReviewId} was not found.");
        }

        if (review.Order.CustomerId != customerIdGuid)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this review.");
        }

        if (request.Content != null)
        {
            review.Content = request.Content;
        }

        if (request.Rating.HasValue)
        {
            if (request.Rating < 1 || request.Rating > 5)
            {
                throw new InvalidOperationException("Rating must be between 1 and 5.");
            }

            review.Rating = request.Rating.Value;
        }

        if (request.ImageUrl != null)
        {
            review.ImageUrl = request.ImageUrl;
        }

        var requestedDetails = request.ReviewDetails?.ToList() ?? [];
        ValidateRequestedRatings(requestedDetails.Where(detail => detail.Rating.HasValue).Select(detail => detail.Rating!.Value));

        if (requestedDetails.Count > 0)
        {
            var reviewDetailIds = requestedDetails.Select(detail => detail.ReviewDetailId).ToHashSet();
            var existingReviewDetails = await _dbContext.ReviewDetails
                .Where(x => x.ReviewId == review.Id && reviewDetailIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            foreach (var detail in requestedDetails)
            {
                if (!existingReviewDetails.TryGetValue(detail.ReviewDetailId, out var reviewDetail))
                {
                    throw new InvalidOperationException($"ReviewDetailId {detail.ReviewDetailId} not found");
                }

                if (detail.DetailContent != null)
                {
                    reviewDetail.DetailContent = detail.DetailContent;
                }

                if (detail.Rating.HasValue)
                {
                    reviewDetail.Rating = detail.Rating.Value;
                }
            }
        }

        var merchant = await _dbContext.Merchants.FirstOrDefaultAsync(x => x.Id == review.MerchantId);
        if (merchant == null)
        {
            throw new KeyNotFoundException("Merchant not found");
        }

        var otherRatings = await _dbContext.Reviews
            .AsNoTracking()
            .Where(x => x.MerchantId == review.MerchantId && x.Id != review.Id)
            .Select(x => x.Rating)
            .ToListAsync();

        merchant.Rating = CalculateAverageRating(otherRatings, review.Rating);
        merchant.UpdatedAt = DateTimeOffset.UtcNow;
        _dbContext.Notifications.Add(new Notification
        {
            UserId = merchant.UserId,
            Title = "Review updated",
            Message = "A customer has updated their review for your merchant.",
            Type = "Review",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        
        await _dbContext.SaveChangesAsync();
    }

    private Guid GetRequiredGuidClaim(string claimType)
    {
        var rawValue = _httpContext.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw new UnauthorizedAccessException($"{claimType} claim is missing");
        }

        return Guid.Parse(rawValue);
    }

    private static void ValidateRequestedRatings(IEnumerable<int> ratings)
    {
        foreach (var rating in ratings)
        {
            if (rating is < 1 or > 5)
            {
                throw new InvalidOperationException("Rating must be between 1 and 5.");
            }
        }
    }

    private static decimal CalculateAverageRating(IEnumerable<int> existingRatings, int newRating)
    {
        var ratings = existingRatings.ToList();
        if (ratings.Count == 0)
        {
            return newRating;
        }

        return (ratings.Sum() + newRating) / (decimal)(ratings.Count + 1);
    }
}
