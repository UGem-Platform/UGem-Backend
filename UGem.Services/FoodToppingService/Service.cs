using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.FoodToppingService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(
        AppDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task CreateFoodTopping(Request.CreateFoodToppingRequest request)
    {
        var merchantId = GetRequiredGuidClaim("MerchantId");

        if (request.Price < 0)
            throw new Exception("Price cannot be negative");

        var food = await _dbContext.Foods
            .FirstOrDefaultAsync(x =>
                x.Id == request.FoodId &&
                !x.IsDeleted);

        if (food == null)
            throw new Exception("Food not found");

        if (food.MerchantId != merchantId)
            throw new UnauthorizedAccessException("You cannot add topping for this food");

        var isExist = await _dbContext.FoodToppings
            .AnyAsync(x =>
                x.FoodId == request.FoodId &&
                x.Name.ToLower() == request.Name.ToLower() &&
                !x.IsDeleted);

        if (isExist)
            throw new Exception("Topping already exists");

        var foodTopping = new FoodTopping
        {
            Id = Guid.NewGuid(),
            FoodId = request.FoodId,
            Name = request.Name,
            Price = request.Price,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        _dbContext.FoodToppings.Add(foodTopping);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Response.FoodToppingResponse>> GetFoodToppings(Guid foodId)
    {
        var food = await _dbContext.Foods
            .FirstOrDefaultAsync(x =>
                x.Id == foodId &&
                !x.IsDeleted);

        if (food == null)
            throw new Exception("Food not found");

        var result = await _dbContext.FoodToppings
            .Where(x =>
                x.FoodId == foodId &&
                x.IsActive &&
                !x.IsDeleted)
            .OrderBy(x => x.Price)
            .Select(x => new Response.FoodToppingResponse
            {
                Id = x.Id,
                FoodId = x.FoodId,
                Name = x.Name,
                Price = x.Price,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return result;
    }

    public async Task UpdateFoodTopping(Request.UpdateFoodToppingRequest request)
    {
        var merchantId = GetRequiredGuidClaim("MerchantId");

        var foodTopping = await _dbContext.FoodToppings
            .Include(x => x.Food)
            .FirstOrDefaultAsync(x =>
                x.Id == request.FoodToppingId &&
                !x.IsDeleted);

        if (foodTopping == null)
            throw new Exception("Food topping not found");

        if (foodTopping.Food.MerchantId != merchantId)
            throw new UnauthorizedAccessException("You cannot update this topping");

        if (!String.IsNullOrWhiteSpace(request.Name))
        {
            var isExistName = await _dbContext.FoodToppings
                .AnyAsync(x =>
                    x.Id != foodTopping.Id &&
                    x.FoodId == foodTopping.FoodId &&
                    x.Name.ToLower() == request.Name.ToLower() &&
                    !x.IsDeleted);

            if (isExistName)
                throw new Exception("Topping name already exists");
            
            foodTopping.Name = request.Name;
        }
        
        if (request.Price.HasValue)
        {
            if (request.Price.Value < 0)
                throw new Exception("Price cannot be negative");

            foodTopping.Price = request.Price.Value;
        }

        if (request.IsActive.HasValue)
        {
            foodTopping.IsActive = request.IsActive.Value;
        }

        foodTopping.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteFoodTopping(Guid foodToppingId)
    {
        var merchantId = GetRequiredGuidClaim("MerchantId");

        var foodTopping = await _dbContext.FoodToppings
            .Include(x => x.Food)
            .FirstOrDefaultAsync(x =>
                x.Id == foodToppingId &&
                !x.IsDeleted);

        if (foodTopping == null)
            throw new Exception("Food topping not found");

        if (foodTopping.Food.MerchantId != merchantId)
            throw new UnauthorizedAccessException("You cannot delete this topping");

        foodTopping.IsDeleted = true;
        foodTopping.IsActive = false;
        foodTopping.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    private Guid GetRequiredGuidClaim(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == claimType)?.Value;

        if (string.IsNullOrWhiteSpace(value))
            throw new UnauthorizedAccessException($"{claimType} not found");

        return Guid.Parse(value);
    }
}