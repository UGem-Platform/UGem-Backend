using Microsoft.EntityFrameworkCore;
using UGem.Repositories;

namespace UGem.Services.Category;

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
            Id = x.Id, Name = x.Name,
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

        var parentExists = await query
            .AnyAsync();

        if (!parentExists)
        {
            throw new Exception("Parent category not found");
        }

        var selectQuery = query.Select(x => new Response.GetCategoryResponse()
        {
            Id = x.Id,
            Name = x.Name,
            ParentId = x.ParentId,
            Description = x.Description,
            Slug = x.Slug
        });
        var listResult = await selectQuery.ToListAsync();
        return listResult;
    }

    public async Task<string> AddCategory(Request.CreateCategoryRequest request)
    {
        var slug = request.Name.ToLower().Trim().Replace(" ", "-");
        var existSlug = await _dbContext.Categories.AnyAsync(x => x.Slug == slug);
        if (existSlug)
        {
            throw new Exception("Category already exists");
        }

        string path;
        if (request.ParentId == null)
        {
            path = "/" + slug;
        }
        else
        {
            var parent = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == request.ParentId);
            if (parent == null)
                throw new Exception("Parent category not found");
            path = parent.Path + "/" + slug;
        }

        var category = new Repositories.Entity.Category
        {
            Name = request.Name,
            Description = request.Description,
            Slug = slug,
            Path = path,
            IsActive =  true
        };
        _dbContext.Categories.Add(category);
        var result = await  _dbContext.SaveChangesAsync();
        return result.ToString();
    }
}