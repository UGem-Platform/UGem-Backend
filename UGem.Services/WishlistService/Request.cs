namespace UGem.Services.WishlistService;

public class Request
{
    public class CreateWishlistRequest
    {
        public Guid MerchantId { get; set; }
    }
}