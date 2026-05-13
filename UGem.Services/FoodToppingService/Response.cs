namespace UGem.Services.FoodToppingService;

public class Response
{
    public class FoodToppingResponse
    {
        public Guid Id { get; set; }

        public Guid FoodId { get; set; }

        public required string Name { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; }
    }
}