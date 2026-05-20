namespace UGem.Services.MerchantService;

public abstract class Response
{
    public class BaseResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Address { get; set; }
        public required string LogoUrl { get; set; }
        public required decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public string? RestaurantType { get; set; }
        public string? MainDishType { get; set; }
        public string? PriceRange { get; set; }
    }

    public class GetMerchantResponse : BaseResponse
    {
        public double? Distance { get; set; }
        public double Latitude { get; set; }          
        public double Longitude { get; set; }
        public decimal UnderratedScore { get; set; }
    }
    
    public class MapResponse : BaseResponse
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class DetailResponse : BaseResponse
    {
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public double Latitude { get; set; }          
        public double Longitude { get; set; }
        public required string OpeningHours { get; set; }
        public decimal UnderratedScore { get; set; }
        public required List<FoodService.Response.Menu> Menu { get; set; }
    }

    public class GetMerchantResponseForStaff: BaseResponse
    {
        public decimal UnderratedScore { get; set; }
        public decimal PlatformFeePercent { get; set; }
       public string? OpeningHours { get; set; }
       public required string Email { get; set; }
    }

    public class MerchantViewResponse
    {
        public Guid MerchantId { get; set; }
        public int TotalViews { get; set; }
    }

    public class MerchantStatisticResponse
    {
        public Guid MerchantId { get; set; }
        public required string MerchantName { get; set; }
        public int TotalViews { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ReviewerFee { get; set; }
        public decimal MerchantReceive { get; set; }
        public decimal AvgOrderValue { get; set; }
        public decimal UnderrateScore { get; set; }
        public decimal PlatformFeePercent { get; set; }
        
    }
}
