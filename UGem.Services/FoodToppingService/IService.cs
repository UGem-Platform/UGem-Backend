namespace UGem.Services.FoodToppingService;

public interface IService
{
        public Task CreateFoodTopping(Request.CreateFoodToppingRequest request);

        public Task<List<Response.FoodToppingResponse>> GetFoodToppings(Guid foodId);

        public Task UpdateFoodTopping(Request.UpdateFoodToppingRequest request);

        public Task DeleteFoodTopping(Guid foodToppingId);
}