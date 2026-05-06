namespace UGem.Services.Category;

public interface IService
{
    public Task<List<Response.GetCategoryResponse>> GetCategories();
    public Task<List<Response.GetCategoryResponse>> GetCategoryById(Guid parentId);
    public Task<string>AddCategory(Request.CreateCategoryRequest request);
}