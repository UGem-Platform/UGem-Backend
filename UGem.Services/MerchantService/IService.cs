namespace UGem.Services.MerchantService;

public interface IService
{
    public Task<Base.Response.PageResult<Response.GetMerchantResponse>> Search(
        string? searchTerm,
        int pageSize,
        int pageIndex);
}