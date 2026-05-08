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

    public async Task<List<Response.MapResponse>> MapRequest(Request.MapRequest request)
    {
        var query = _dbContext.Merchants.AsQueryable();

        query = query.Where(m =>
            m.Longitude >= request.MinLongitude &&
            m.Longitude <= request.MaxLongitude &&
            m.Latitude >= request.MinLatitude &&
            m.Latitude <= request.MaxLatitude);

        if (request.ZoomLevel < 13)
        {
            query = query.Where(m => (m.Reviews.Average(r => (decimal?)r.Rating) ?? 0m) >= 4.5m);
            query = query.OrderByDescending(m => m.Reviews.Average(r => (decimal?)r.Rating) ?? 0m).Take(50);
        }
        else
        {
            query = query.OrderByDescending(m => m.Reviews.Average(r => (decimal?)r.Rating) ?? 0m).Take(100);
        }

        var result = await query.Select(m => new Response.MapResponse()
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            Address = m.Address,
            LogoUrl = m.LogoUrl,
            Rating = m.Reviews.Average(r => (decimal?)r.Rating) ?? 0m,
            ReviewCount = m.Reviews.Count(),
            RestaurantType = m.RestaurantType,
            MainDishType = m.MainDishType,
            PriceRange = m.PriceRange,
            Latitude = m.Latitude,
            Longitude = m.Longitude
        }).ToListAsync();

        return result;
    }

    public async Task<Base.Response.PageResult<Response.GetMerchantResponse>> Search(Request.SearchRequest request)
    {
        var queryBySearch = _dbContext.Merchants.AsQueryable();

        if (request.SearchTerm != null)
        {
            queryBySearch = queryBySearch.Where(m =>
                EF.Functions.ILike(m.Name, $"%{request.SearchTerm}%") ||
                EF.Functions.ILike(m.Description, $"%{request.SearchTerm}%") ||
                m.Foods.Any(f =>
                    EF.Functions.ILike(f.Name, $"%{request.SearchTerm}%") ||
                    EF.Functions.ILike(f.Description, $"%{request.SearchTerm}%")
                )
            );
        }

        var userLocation = new NetTopologySuite.Geometries.Point(request.Longitude, request.Latitude) { SRID = 4326 };

        var queryByDistance =
            queryBySearch.Where(m => m.Location.IsWithinDistance(userLocation, 15000)); // 15 km radius

        var totalItems = await queryByDistance.CountAsync();

        var queryOrder = queryByDistance
            .OrderByDescending(m => m.UnderratedScore)
            .ThenBy(m => m.Location.Distance(userLocation));

        var query = queryOrder
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize);

        var selectedQuery = query
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
            });

        var listResult = await selectedQuery
            .ToListAsync();

        var result = new Base.Response.PageResult<Response.GetMerchantResponse>()
        {
            Items = listResult,
            TotalItems = totalItems,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
        };

        return result;
    }

    public async Task<Response.DetailResponse?> GetDetail(Guid id)
    {
        var query = _dbContext.Merchants.Where(m => m.Id == id);

        var selectedQuery = query.Select(x => new Response.DetailResponse()
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
            Latitude =  x.Latitude,
            Longitude =  x.Longitude,
            Menu = x.Foods.Select(f => new FoodService.Response.Menu()
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Price = f.Price,
                ImageUrl = f.ImageUrl,
                CategoryDetail = f.CategoryDetails.Select(cd => cd.Category.Name).ToList()
            }).ToList()
        });

        var result = await selectedQuery.FirstOrDefaultAsync();

        return result;
    }

    public async Task<Base.Response.PageResult<Response.GetMerchantResponse>> GetMerchantByCategory(Request.GetByCategoryRequest request)
    {
        var queryBySearch = _dbContext.Merchants.AsQueryable();

        if (request.CategoryId != Guid.Empty)
        {
            queryBySearch = queryBySearch.Where(m =>
                m.Foods.Any(f => f.CategoryDetails.Any(cd => cd.CategoryId == request.CategoryId)));
        }

        var userLocation = new NetTopologySuite.Geometries.Point(request.Longitude, request.Latitude) { SRID = 4326 };

        var queryByDistance =
            queryBySearch.Where(m => m.Location.IsWithinDistance(userLocation, 15000)); // 15 km radius

        var totalItems = await queryByDistance.CountAsync();

        var queryOrder = queryByDistance
            .OrderBy(m => m.Location.Distance(userLocation));

        var query = queryOrder
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize);

        var selectedQuery = query
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
            });

        var listResult = await selectedQuery
            .ToListAsync();

        var result = new Base.Response.PageResult<Response.GetMerchantResponse>()
        {
            Items = listResult,
            TotalItems = totalItems,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
        };

        return result;
    }

    public async Task UpdateMerchant(Request.UpdateMerchantRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;

        var userIdGuid = Guid.Parse(userId!);

        var merchant = await _dbContext.Merchants
            .FirstOrDefaultAsync(m => m.UserId == userIdGuid);

        if (merchant == null)
            throw new Exception("Merchant not found or not yours");
        if (!string.IsNullOrWhiteSpace(request.MerchantName))
        {
            merchant.Name = request.MerchantName;
        }
        if(!string.IsNullOrWhiteSpace(request.MerchantDescription))
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
        if (request.PriceRange.HasValue)
        {
            merchant.PriceRange = request.PriceRange.Value;
        }
        if(!string.IsNullOrWhiteSpace(request.Email))
        {
            merchant.Email = request.Email;
        }
        if(!string.IsNullOrWhiteSpace(request.Phone))
        {
            merchant.Phone = request.Phone;
        }
        if(!string.IsNullOrWhiteSpace(request.Address))
        {
            merchant.Address = request.Address;
        }
        if(!string.IsNullOrWhiteSpace(request.OpeningHours))
        {
            merchant.OpeningHours = request.OpeningHours;
        }
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Response.BaseResponse>> FilterOrdersByPrice(
        Request.GetOrderListMaxMinRequest request)
    {
        if (request.MinPrice.HasValue &&
            request.MaxPrice.HasValue &&
            request.MinPrice > request.MaxPrice)
        {
            throw new Exception(
                "Min price must be less than or equal to max price");
        }

        var query = _dbContext.Merchants.AsQueryable();
        
        if (request.MinPrice.HasValue)
        {
            query = query.Where(m =>
                m.PriceRange >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(m =>
                m.PriceRange <= request.MaxPrice.Value);
        }

        var selectedQuery = query
            .OrderByDescending(m =>
                m.Reviews.Average(r => (decimal?)r.Rating) ?? 0m)
            .Select(m => new Response.BaseResponse
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Address = m.Address,
                LogoUrl = m.LogoUrl,
                Rating = m.Reviews.Average(r =>
                    (decimal?)r.Rating) ?? 0m,
                ReviewCount = m.Reviews.Count(),
                RestaurantType = m.RestaurantType,
                MainDishType = m.MainDishType,
                PriceRange = m.PriceRange,
            });

        return await selectedQuery.ToListAsync();
    }
}
