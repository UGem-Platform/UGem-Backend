namespace UGem.Services.MerchantService;

public class Request
{
    public class MapRequest
    {
        public double MinLongitude { get; set; }
        public double MaxLongitude { get; set; }
        public double MinLatitude { get; set; }
        public double MaxLatitude { get; set; }
        public double ZoomLevel { get; set; }
    }
    public class SearchRequest : Base.Request.PageRequest
    {
        public string? SearchTerm { get; set; }
    }
    
    public class GetByCategoryRequest : Base.Request.PageRequest
    {
        public Guid CategoryId { get; set; }
    }
    public class UpdateMerchantRequest
    {
       
        public string? MerchantName { get; set; }
        public string? MerchantDescription { get; set; }
        public string? RestaurantType { get; set; }
        public string? MainDishType { get; set; }
        public decimal? PriceRange { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? OpeningHours { get; set; }
        
        
    }
    
    public class GetOrderListMaxMinRequest
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
