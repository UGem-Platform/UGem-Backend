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

        var reviews = _dbContext.Reviews.Where(x => x.MerchantId == request.MerchantId);

        var selectedQuery = reviews.Select(x => new Response.ReviewsByIdMerchantResponse()
        {
            Content = x.Content,
            Rating =  x.Rating,
            CreatedAt = x.CreatedAt
        });
        
        var result = await selectedQuery.ToListAsync();
        
        return result;
    }

    public async Task ReviewMerchant(Request.ReviewByMerchantIdRequest request)
    {
        var customerId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;
        
        var customerIdGuid = Guid.Parse(customerId!);

        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.CustomerId == customerIdGuid
                                                                     && x.Id == request.OrderId);
        
        
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {request.OrderId} not found");
        }

        if (order.Status != "Completed")
        {
            throw new InvalidOperationException("Order already completed");
        }
        
        var existedReview = await _dbContext.Reviews
            .FirstOrDefaultAsync(x => x.OrderId == request.OrderId);

        if (existedReview != null)
        {
            throw new InvalidOperationException("Order has been reviewed.");
        }

        var newReview = new Review()
        {
            Id = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            OrderId = request.OrderId,
            Content = request.Content,
            Rating = request.Rating,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        
        _dbContext.Add(newReview);
        var result = await _dbContext.SaveChangesAsync();

        if (result > 0 && request.ReviewDetails != null && request.ReviewDetails.Any())
        {
            List<ReviewDetail> newReviewDetail = new List<ReviewDetail>();

            foreach (var detail in request.ReviewDetails)
            {    
                var isValid = await _dbContext.OrderDetails
                    .AnyAsync(x => x.Id == detail.OrderDetailId 
                                   && x.OrderId == request.OrderId);

                if (!isValid)
                {
                    throw new InvalidOperationException($"OrderDetailId {detail.OrderDetailId} unValid");
                }
                if (detail.Rating < 1 || detail.Rating > 5)
                {
                    throw new InvalidOperationException("Rating must be between 1 and 5.");
                }
                
                newReviewDetail.Add(new ReviewDetail()
                {
                    Id = Guid.NewGuid(),
                    ReviewId = newReview.Id,
                    OrderDetailId = detail.OrderDetailId,
                    DetailContent = detail.DetailContent,
                    Rating = detail.Rating,
                });
            }

            _dbContext.AddRange(newReviewDetail);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateReviewMerchant(Request.UpdateReviewByMerchantIdRequest request)
    {
        var customerId = _httpContext.HttpContext.User.Claims
            .FirstOrDefault(x => x.Type == "CustomerId")?.Value;

        var customerIdGuid = Guid.Parse(customerId!);

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
            review.Content = request.Content;

        if (request.Rating.HasValue)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new InvalidOperationException("Rating must be between 1 and 5.");

            review.Rating = request.Rating.Value;
        }
        
        if (request.ImageUrl != null)
            review.ImageUrl = request.ImageUrl;
        

        if (request.ReviewDetails != null && request.ReviewDetails.Any())
        {
            foreach (var detail in request.ReviewDetails)
            {
                var reviewDetail = await _dbContext.ReviewDetails.FirstOrDefaultAsync(x => x.Id == detail.ReviewDetailId);

                if (reviewDetail == null)
                {
                    throw new InvalidOperationException($"ReviewDetailId {detail.ReviewDetailId} not found");
                }
                
                if (detail.DetailContent != null)
                    reviewDetail.DetailContent = detail.DetailContent;

                if (detail.Rating.HasValue)
                {
                    if (detail.Rating < 1 || detail.Rating > 5)
                        throw new InvalidOperationException("Rating must be between 1 and 5.");

                    reviewDetail.Rating = detail.Rating.Value;
                }
            }
            
        }
        await _dbContext.SaveChangesAsync();

    }
}