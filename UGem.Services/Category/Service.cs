using Microsoft.EntityFrameworkCore;
using UGem.Repositories;

namespace UGem.Service.Category;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<List<Response.GetCategoryResponse>> GetCategories()
    {
        var query = _dbContext.Categories.OrderBy(c => c.Name);
        var selectQuery = query.Select(x => new Response.GetCategoryResponse()
        {
            Id = x.Id, Name = x.Name ,
            ParentId = x.ParentId,
            Description = x.Description,
            Slug = x.Slug
        });
        var result = await selectQuery.ToListAsync();
        return result;
    }

    public async Task<List<Response.GetCategoryResponse>> GetCategoryById(Guid parentId)
    {
        var query = _dbContext.Categories.Where(x => x.Id == parentId);
        var selectQuery = query.Select(x => new Response.GetCategoryResponse()
        {
            Id = x.Id,
            Name = x.Name,
            ParentId = x.ParentId,
            Description = x.Description,
            Slug = x.Slug
        });
        var listResult = await  selectQuery.ToListAsync();
        return listResult;
    }
}