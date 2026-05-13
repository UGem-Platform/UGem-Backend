namespace UGem.Services.FoodToppingService;

public class Request
{
    public class CreateFoodToppingRequest
    {
        public Guid FoodId { get; set; }

        public required string Name { get; set; }

        public decimal Price { get; set; }
    }
    public class UpdateFoodToppingRequest
    {
        public Guid FoodToppingId { get; set; }

        public string? Name { get; set; }

        public decimal? Price { get; set; }

        public bool? IsActive { get; set; }
    }
}