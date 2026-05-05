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
                    throw new InvalidOperationException("Rating phải từ 1 đến 5");
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

}