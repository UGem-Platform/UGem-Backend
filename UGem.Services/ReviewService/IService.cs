namespace UGem.Services.ReviewService;

public interface IService
{
        public Task<Base.Response.PageResult<Response.GetReviewsResponse>> GetReviews(
            Guid? merchantId,
            int pageSize,
            int pageIndex);
        
        public Task<Response.GetReviewsResponse?> GetReviewById(Guid id);
}