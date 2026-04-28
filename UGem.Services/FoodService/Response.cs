namespace UGem.Services.FoodService;

public class Response
{
    public class Menu
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public List<string>? CategoryDetail { get; set; }
    }
}