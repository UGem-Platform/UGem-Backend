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

        var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var envelope = new NetTopologySuite.Geometries.Envelope(request.MinLongitude, request.MaxLongitude,
            request.MinLatitude, request.MaxLatitude);
        var bbox = geometryFactory.ToGeometry(envelope);

        query = query.Where(m => m.Location.Intersects(bbox));

        if (request.ZoomLevel < 13)
        {
            query = query.Where(m => m.Rating >= 4.5m);
            query = query.OrderByDescending(m => m.Rating).Take(50);
        }
        else
        {
            query = query.OrderByDescending(m => m.Rating).Take(100);
        }

        var electedQuery = query.Select(m => new Response.MapResponse()
        {
            Id = m.Id,
            Name = m.Name,
            Rating = m.Rating,
            Latitude = m.Location.Y,
            Longitude = m.Location.X
        });

        var result = await electedQuery.ToListAsync();

        return result;
    }

    public async Task<Base.Response.PageResult<Response.GetMerchantResponse>> Search(Request.SearchRequest request)
    {
        var queryBySearch = _dbContext.Merchants.Where(m => true);

        if (request.SearchTerm != null)
        {
            queryBySearch = queryBySearch.Where(m =>
                m.Name.Contains(request.SearchTerm ?? "") ||
                m.Description.Contains(request.SearchTerm ?? "") ||
                m.Foods.Any(f =>
                    f.Name.Contains(request.SearchTerm ?? "") ||
                    f.Description.Contains(request.SearchTerm ?? "")
                )
            );
        }

        var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var userLocation =
            geometryFactory.CreatePoint(
                new NetTopologySuite.Geometries.Coordinate(request.Longitude, request.Latitude));

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
                Rating = x.Rating,
                Distance = x.Location.Distance(userLocation) / 1000,
                Address = x.Address,
                Latitude = x.Location.Y,
                Longitude = x.Location.X,
                
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
            Rating = x.Rating,
            Description = x.Description,
            Email = x.Email,
            Phone = x.Phone,
            Address = x.Address,
            LogoUrl = x.LogoUrl,
            Latitude =  x.Location.Y,
            Longitude =  x.Location.X,
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
        var queryBySearch = _dbContext.Merchants.Where(m => true);

        

        var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var userLocation =
            geometryFactory.CreatePoint(
                new NetTopologySuite.Geometries.Coordinate(request.Longitude, request.Latitude));

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
                Rating = x.Rating,
                Latitude = x.Location.Y,
                Longitude = x.Location.X,
                Distance = x.Location.Distance(userLocation) / 1000,
                Address = x.Address,
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
}