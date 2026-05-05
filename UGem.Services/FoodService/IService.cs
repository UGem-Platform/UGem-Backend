namespace UGem.Services.FoodService;

public interface IService
{
    public Task<string> CreateFood(Request.CreateFoodRequest request);
    
}