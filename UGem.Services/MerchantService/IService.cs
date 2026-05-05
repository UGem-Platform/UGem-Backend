namespace UGem.Services.MerchantService;

public interface IService
{
    public Task<List<Response.MapResponse>> MapRequest(Request.MapRequest request);
    public Task<Base.Response.PageResult<Response.GetMerchantResponse>> Search(Request.SearchRequest request);
    public Task<Response.DetailResponse?> GetDetail(Guid id);
    public Task<Base.Response.PageResult<Response.GetMerchantResponse>> GetMerchantByCategory(Request.GetByCategoryRequest request);
    public Task UpdateMerchant(Request.UpdateMerchantRequest request);
}