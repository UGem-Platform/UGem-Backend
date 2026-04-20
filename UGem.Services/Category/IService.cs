namespace UGem.Service.Category;

public interface IService
{
    public Task<List<Response.GetCategoryResponse>> GetCategories();
    public Task<List<Response.GetCategoryResponse>> GetCategoryById(Guid parentId);
}