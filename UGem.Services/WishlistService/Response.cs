namespace UGem.Service.WishlistService;

public class Response
{
    public class WishlistItemResponse
    {
        public string Name { get; set; } = "";
        public string LogoUrl { get; set; } = "";
        public decimal Rating { get; set; }
    }
}