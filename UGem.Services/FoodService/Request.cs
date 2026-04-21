namespace UGem.Services.FoodService;

public class Request
{
    public class CreateFoodRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
    }
}