using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UGem.Api.Extensions;
using UGem.Services.WishlistService;

namespace UGem.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WishlistController : ControllerBase
{   
    private readonly IService _service;

    public WishlistController(IService service)
    {
        _service = service;
    }
    //add 1 merchant to your wish list.
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpPost("")]
    public async Task<IActionResult> AddToWishlist(Request.CreateWishlistRequest request)
    {
        var rs = await _service.AddToWishlist(request);
        return Ok(rs);
    }

    //show all wish list
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpGet("")]
    public async Task<IActionResult> GetWishlist()
    {
        var rs = await _service.GetWishlist();
        return Ok(rs);
    }
    
    //delete 1 merchant from wishlist
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [HttpDelete("{merchantId}")]
    public async Task<IActionResult> RemoveFromWishlist(Guid merchantId)
    {
        var rs = await _service.RemoveFromWishlist(merchantId);
        return Ok(rs);
    }    
}