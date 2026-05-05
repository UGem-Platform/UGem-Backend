using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

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

    public async Task<string> CreateFood(Request.CreateFoodRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new Exception("Unauthorized");
        var userIdGuid = Guid.Parse(userId!);

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
        return "Create food Successfully";
    }
}