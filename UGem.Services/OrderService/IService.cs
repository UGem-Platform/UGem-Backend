using UGem.Repositories.Entity;

namespace UGem.Services.OrderService;

public interface IService
{
    public Task<List<Response.GetOrderListResponse>> GetOrdersList();
    public Task CreateOrder(Request.CreateOrderRequest request);
}