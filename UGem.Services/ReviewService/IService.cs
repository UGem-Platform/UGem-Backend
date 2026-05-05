namespace UGem.Services.ReviewService;

public interface IService
{
        public Task<Base.Response.PageResult<Response.GetReviewsResponse>> GetReviews(
            Guid? merchantId,
            int pageSize,
            int pageIndex);
        
        public Task<List<Response.ReviewsByIdMerchantResponse>> GetReviewByMerchantId(Request.GetReviewByMerchantIdRequest request);
        
        public Task ReviewMerchant(Request.ReviewByMerchantIdRequest request);
}