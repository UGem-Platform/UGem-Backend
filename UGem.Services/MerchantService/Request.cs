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
}