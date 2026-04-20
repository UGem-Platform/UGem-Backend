namespace UGem.Service.WishlistService;

public interface IService
{
    public Task<string> AddToWishlist(Request.CreateWishlistRequest request);
    Task<List<Response.WishlistItemResponse>> GetWishlist();
    Task<string> RemoveFromWishlist(Guid merchantId);
}