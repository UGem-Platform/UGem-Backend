namespace UGem.Services.MerchantService;

public abstract class Response
{
    public class BaseResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required decimal Rating { get; set; }
    }

    public class GetMerchantResponse : BaseResponse
    {
        public double? Distance { get; set; }
    }
    
    public class MapResponse : BaseResponse
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class DetailResponse : BaseResponse
    {
        public required string Description { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Address { get; set; }
        public required string LogoUrl { get; set; }
        public required List<FoodService.Response.Menu> Menu { get; set; }
    }
}