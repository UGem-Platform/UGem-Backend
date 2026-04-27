using UGem.Repositories.Entity;

namespace UGem.Services.OrderService;

public interface IService
{
    public Task<List<Response.GetOrderListResponse>> GetOrdersList( );
    public Task CreateOrder(Request.CreateOrderRequest request);
    public Task<List<Response.OrderResponse>> GetOrderListFromCustomerId();
    public Task<List<Response.GetOrderDetailResponse>> GetOrderDetail(Guid orderId);

}