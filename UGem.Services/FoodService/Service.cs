using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;


namespace UGem.Services.FoodService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task<string> CreateFood(Request.AddFoodRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new Exception("Unauthorized");
        var userIdGuid = Guid.Parse(userId);

        var merchant = await _dbContext.Merchants
            .FirstOrDefaultAsync(m => m.UserId == userIdGuid);
        if (merchant == null)
        {
            throw new Exception("Merchant not found");
        }
        if (request.Price <= 0)
            throw new Exception("Price must be greater than 0");

        var food = new Repositories.Entity.Food
        {
            Name = request.Name,
            Description = request.Description,
            MerchantId = merchant.Id,
            Price = request.Price,
            IsAvailable = true
        };
        _dbContext.Foods.Add(food);
        await _dbContext.SaveChangesAsync();
        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            var validCategories = await _dbContext.Categories
                .Where(c => request.CategoryIds.Contains(c.Id))
                .ToListAsync();

            foreach (var category in validCategories)
            {
                _dbContext.CategoryDetails.Add(new Repositories.Entity.CategoryDetail
                {
                    CategoryId = category.Id,
                    FoodId = food.Id,
                    Name = food.Name,
                    ImgUrl = request.ImageUrl ?? "",
                    Description = request.Description
                });
            }
            await _dbContext.SaveChangesAsync();
        }
        return "Create food Successfully";
    }

    public async Task DeleteFood(Guid foodId)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new Exception("Unauthorized");
        var userIdGuid = Guid.Parse(userId);

        var merchant = await _dbContext.Merchants
            .FirstOrDefaultAsync(m => m.UserId == userIdGuid);
        if (merchant == null)
        {
            throw new Exception("Merchant not found");
        }
        var food = await _dbContext.Foods.Include(x => x.Merchant).FirstOrDefaultAsync(x => x.Id == foodId);
        if(food == null)
            {
            throw new Exception("Food not found");
            }
        if(food.Merchant.UserId != userIdGuid)
            {
            throw new Exception("Merchant is not allowed to delete this food");
            }
        food.IsAvailable = false;
        food.IsDeleted = true;
        food.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
}