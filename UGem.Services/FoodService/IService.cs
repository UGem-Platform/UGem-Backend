namespace UGem.Services.FoodService;

public interface IService
{
    public Task<string> CreateFood(Request.AddFoodRequest request);
    public Task DeleteFood(Guid foodId);
}