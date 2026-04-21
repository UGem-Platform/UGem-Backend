namespace UGem.Services.Application;

public class Request
{
    public class ApplicationRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public decimal Latitude { get; set; }   // vĩ độ
        public decimal Longitude { get; set; }  // kinh độ
        
        public List<FoodService.Request.CreateFoodRequest>? Menu { get; set; }
    }
    public class RejectApplicationRequest
    {
        public Guid ApplicationId { get; set; }
        public string Note { get; set; } = "";
    }
    
    public class UpdateApplicationRequest :ApplicationRequest
    {
        public Guid ApplicationId { get; set; }
        public required string Type { get; set; }
        public string? Note { get; set; }
    }
}