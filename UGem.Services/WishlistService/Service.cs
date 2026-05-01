using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.WishlistService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task AddToWishlist(Request.CreateWishlistRequest request)
    {
        var customerId = _httpContext.HttpContext.User.Claims.FirstOrDefault(
            x => x.Type == "CustomerId")?.Value;
        
        var customerIdGuid = Guid.Parse(customerId!);
       
        if (request.MerchantId == Guid.Empty)
        {
            throw new Exception("MerchantId is required");
        }

        
        var wishlist = await _dbContext.Wishlists
            .FirstOrDefaultAsync(x => x.CustomerId == customerIdGuid);

        if (wishlist == null)
        {
            wishlist = new Wishlist
            {
                Id = Guid.NewGuid(),
                CustomerId = customerIdGuid,
            };

            _dbContext.Wishlists.Add(wishlist);
            await _dbContext.SaveChangesAsync();
        }
        
        var isExist = await _dbContext.WishlistDetails
            .AnyAsync(x => x.WishlistId == wishlist.Id &&
                           x.MerchantId == request.MerchantId);

        if (isExist)
        {
            throw new Exception("Merchant already in wishlist");
        }

        var detail = new WishlistDetail
        {
            Id = Guid.NewGuid(),
            WishlistId = wishlist.Id,
            MerchantId = request.MerchantId,
        };

        _dbContext.WishlistDetails.Add(detail);
        await _dbContext.SaveChangesAsync();
    }
    public async Task<List<Response.WishlistItemResponse>> GetWishlist()
    {
        var customerId = _httpContext.HttpContext.User.Claims
            .FirstOrDefault(x => x.Type == "CustomerId")?.Value;

        var customerIdGuid = Guid.Parse(customerId!);

        var wishlist = await _dbContext.Wishlists
            .FirstOrDefaultAsync(x => x.CustomerId == customerIdGuid);

        if (wishlist == null)
        {
            throw new Exception("don't have a wishlist");
        }

        var result = await _dbContext.WishlistDetails
            .Where(x => x.WishlistId == wishlist.Id)
            .Select(x => new Response.WishlistItemResponse
            {
                Name = x.Merchant.Name,
                LogoUrl = x.Merchant.LogoUrl,
                Rating = x.Merchant.Rating
            })
            .ToListAsync();

        return result;
    }
    
    public async Task RemoveFromWishlist(Guid merchantId)
    {
        var customerId = _httpContext.HttpContext.User.Claims
            .FirstOrDefault(x => x.Type == "CustomerId")?.Value;

        var customerIdGuid = Guid.Parse(customerId!);

        var wishlist = await _dbContext.Wishlists
            .FirstOrDefaultAsync(x => x.CustomerId == customerIdGuid);

        if (wishlist == null)
        {
            throw new Exception("Wishlist not found");
        }

        var detail = await _dbContext.WishlistDetails
            .FirstOrDefaultAsync(x => x.WishlistId == wishlist.Id 
                                      && x.MerchantId == merchantId);

        if (detail == null)
        {
            throw new Exception("Merchant not in wishlist");
        }

        _dbContext.Remove(detail);
        await _dbContext.SaveChangesAsync();
    }
}