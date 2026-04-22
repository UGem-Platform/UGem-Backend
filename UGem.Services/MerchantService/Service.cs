using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;

namespace UGem.Services.MerchantService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task<Base.Response.PageResult<Response.GetMerchantResponse>> Search(string? searchTerm, int pageSize,
        int pageIndex)
    {
        var query = _dbContext.Merchants.Where(m => true);

        if (searchTerm != null)
        {
            query = query.Where(m =>
                m.Name.Contains(searchTerm ?? "") ||
                m.Description.Contains(searchTerm ?? "") ||
                m.Foods.Any(f =>
                    f.Name.Contains(searchTerm ?? "") ||
                    f.Description.Contains(searchTerm ?? "")
                )
            );
        }

        query = query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize);

        var selectedQuery = query
            .Select(x => new Response.GetMerchantResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Rating = x.Rating,
            });

        var listResult = await selectedQuery.ToListAsync();
        var totalItem = listResult.Count;

        var result = new Base.Response.PageResult<Response.GetMerchantResponse>()
        {
            Items = listResult,
            TotalItems = totalItem,
            PageIndex = pageIndex,
            PageSize = pageSize,
        };
        
        return result;
    }
}