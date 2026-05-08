using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using UGem.Repositories;

namespace UGem.Services.MerchantService;

public class Service : IService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task<List<Response.MapResponse>> MapRequest(Request.MapRequest request)
    {
        var query = _dbContext.Merchants
            .AsNoTracking()
            .Where(m =>
                m.Longitude >= request.MinLongitude &&
                m.Longitude <= request.MaxLongitude &&
                m.Latitude >= request.MinLatitude &&
                m.Latitude <= request.MaxLatitude)
            .Select(m => new
            {
                Merchant = m,
                Rating = m.Reviews.Average(r => (decimal?)r.Rating) ?? 0m,
                ReviewCount = m.Reviews.Count()
            });

        if (request.ZoomLevel < 13)
        {
            query = query
                .Where(x => x.Rating >= 4.5m)
                .OrderByDescending(x => x.Rating)
                .Take(50);
        }
        else
        {
            query = query
                .OrderByDescending(x => x.Rating)
                .Take(100);
        }

        return await query
            .Select(x => new Response.MapResponse
            {
                Id = x.Merchant.Id,
                Name = x.Merchant.Name,
                Description = x.Merchant.Description,
                Address = x.Merchant.Address,
                LogoUrl = x.Merchant.LogoUrl,
                Rating = x.Rating,
                ReviewCount = x.ReviewCount,
                RestaurantType = x.Merchant.RestaurantType,
                MainDishType = x.Merchant.MainDishType,
                PriceRange = x.Merchant.PriceRange,
                Latitude = x.Merchant.Latitude,
                Longitude = x.Merchant.Longitude
            })
            .ToListAsync();
    }

    public async Task<Base.Response.PageResult<Response.GetMerchantResponse>> Search(Request.SearchRequest request)
    {
        var (pageIndex, pageSize) = NormalizePagination(request.PageIndex, request.PageSize);
        var queryBySearch = _dbContext.Merchants.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            queryBySearch = queryBySearch.Where(m =>
                EF.Functions.ILike(m.Name, $"%{request.SearchTerm}%") ||
                EF.Functions.ILike(m.Description, $"%{request.SearchTerm}%") ||
                m.Foods.Any(f =>
                    EF.Functions.ILike(f.Name, $"%{request.SearchTerm}%") ||
                    EF.Functions.ILike(f.Description, $"%{request.SearchTerm}%")));
        }

        var userLocation = new Point(request.Longitude, request.Latitude) { SRID = 4326 };

        var queryByDistance = queryBySearch
            .Where(m => m.Location.IsWithinDistance(userLocation, 15000));

        var totalItems = await queryByDistance.CountAsync();

        var items = await queryByDistance
            .OrderByDescending(m => m.UnderratedScore)
            .ThenBy(m => m.Location.Distance(userLocation))
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Response.GetMerchantResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Address = x.Address,
                LogoUrl = x.LogoUrl,
                Rating = x.Reviews.Average(r => (decimal?)r.Rating) ?? 0m,
                ReviewCount = x.Reviews.Count(),
                RestaurantType = x.RestaurantType,
                MainDishType = x.MainDishType,
                PriceRange = x.PriceRange,
                Distance = x.Location.Distance(userLocation) / 1000,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                UnderratedScore = x.UnderratedScore
            })
            .ToListAsync();

        return new Base.Response.PageResult<Response.GetMerchantResponse>
        {
            Items = items,
            TotalItems = totalItems,
            PageIndex = pageIndex,
            PageSize = pageSize,
        };
    }

    public async Task<Response.DetailResponse?> GetDetail(Guid id)
    {
        return await _dbContext.Merchants
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(x => new Response.DetailResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Address = x.Address,
                LogoUrl = x.LogoUrl,
                Rating = x.Reviews.Average(r => (decimal?)r.Rating) ?? 0m,
                ReviewCount = x.Reviews.Count(),
                RestaurantType = x.RestaurantType,
                MainDishType = x.MainDishType,
                PriceRange = x.PriceRange,
                Email = x.Email,
                Phone = x.Phone,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                Menu = x.Foods.Select(f => new FoodService.Response.Menu
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Price = f.Price,
                    ImageUrl = f.ImageUrl,
                    CategoryDetail = f.CategoryDetails.Select(cd => cd.Category.Name).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Base.Response.PageResult<Response.GetMerchantResponse>> GetMerchantByCategory(Request.GetByCategoryRequest request)
    {
        var (pageIndex, pageSize) = NormalizePagination(request.PageIndex, request.PageSize);
        var queryBySearch = _dbContext.Merchants.AsNoTracking().AsQueryable();

        if (request.CategoryId != Guid.Empty)
        {
            queryBySearch = queryBySearch.Where(m =>
                m.Foods.Any(f => f.CategoryDetails.Any(cd => cd.CategoryId == request.CategoryId)));
        }

        var userLocation = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
        var queryByDistance = queryBySearch
            .Where(m => m.Location.IsWithinDistance(userLocation, 15000));

        var totalItems = await queryByDistance.CountAsync();

        var items = await queryByDistance
            .OrderByDescending(m => m.UnderratedScore)
            .ThenBy(m => m.Location.Distance(userLocation))
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Response.GetMerchantResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Address = x.Address,
                LogoUrl = x.LogoUrl,
                Rating = x.Reviews.Average(r => (decimal?)r.Rating) ?? 0m,
                ReviewCount = x.Reviews.Count(),
                RestaurantType = x.RestaurantType,
                MainDishType = x.MainDishType,
                PriceRange = x.PriceRange,
                Distance = x.Location.Distance(userLocation) / 1000,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
            })
            .ToListAsync();

        return new Base.Response.PageResult<Response.GetMerchantResponse>
        {
            Items = items,
            TotalItems = totalItems,
            PageIndex = pageIndex,
            PageSize = pageSize,
        };
    }

    public async Task UpdateMerchant(Request.UpdateMerchantRequest request)
    {
        var userIdGuid = GetRequiredGuidClaim("UserId");

        var merchant = await _dbContext.Merchants
            .FirstOrDefaultAsync(m => m.UserId == userIdGuid);

        if (merchant == null)
        {
            throw new KeyNotFoundException("Merchant not found or not yours");
        }

        if (!string.IsNullOrWhiteSpace(request.MerchantName))
        {
            merchant.Name = request.MerchantName;
        }

        if (!string.IsNullOrWhiteSpace(request.MerchantDescription))
        {
            merchant.Description = request.MerchantDescription;
        }

        if (!string.IsNullOrWhiteSpace(request.RestaurantType))
        {
            merchant.RestaurantType = request.RestaurantType;
        }

        if (!string.IsNullOrWhiteSpace(request.MainDishType))
        {
            merchant.MainDishType = request.MainDishType;
        }

        if (!string.IsNullOrWhiteSpace(request.PriceRange))
        {
            merchant.PriceRange = request.PriceRange;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            merchant.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            merchant.Phone = request.Phone;
        }

        if (!string.IsNullOrWhiteSpace(request.Address))
        {
            merchant.Address = request.Address;
        }

        if (!string.IsNullOrWhiteSpace(request.OpeningHours))
        {
            merchant.OpeningHours = request.OpeningHours;
        }

        await _dbContext.SaveChangesAsync();
    }

    private static (int PageIndex, int PageSize) NormalizePagination(int pageIndex, int pageSize)
    {
        var normalizedPageIndex = pageIndex <= 0 ? 1 : pageIndex;
        var normalizedPageSize = pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
        return (normalizedPageIndex, normalizedPageSize);
    }

    private Guid GetRequiredGuidClaim(string claimType)
    {
        var rawValue = _httpContext.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw new UnauthorizedAccessException($"{claimType} claim is missing");
        }

        return Guid.Parse(rawValue);
    }
}
