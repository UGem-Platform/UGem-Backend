namespace UGem.Services.WishlistService;

public interface IService
{
    public Task AddToWishlist(Request.CreateWishlistRequest request);
    Task<List<Response.WishlistItemResponse>> GetWishlist();
    Task RemoveFromWishlist(Guid merchantId);
}