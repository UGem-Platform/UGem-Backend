using Microsoft.AspNetCore.Http;

namespace UGem.Services.FoodService;

public class Request
{
    public class CreateFoodRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public List<Guid>? CategoryIds { get; set; }
    }

    public class AddFoodRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public List<Guid>? CategoryIds { get; set; }
    }

    public class FoodOrderRequest
    {
        public Guid FoodId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
}