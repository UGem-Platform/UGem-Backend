using UGem.Repositories;

namespace UGem.Service.ReviewService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Base.Response.PageResult<Response.GetReviewsResponse>> GetReviews(Guid? merchantId, int pageSize, int pageIndex)
    {
        throw new NotImplementedException();
    }

    public Task<Response.GetReviewsResponse?> GetReviewById(Guid id)
    {
        throw new NotImplementedException();
    }
}