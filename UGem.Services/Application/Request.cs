namespace UGem.Services.Application;

public class Request
{
    public class CreateApplicationRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public decimal Latitude { get; set; }   // vĩ độ
        public decimal Longitude { get; set; }  // kinh độ
        
        public required List<FoodService.Request.CreateFoodRequest> Menu { get; set; }
    }
}