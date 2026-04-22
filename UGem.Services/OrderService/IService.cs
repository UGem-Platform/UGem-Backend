using UGem.Repositories.Entity;

namespace UGem.Services.OrderService;

public interface IService
{
    public Task<List<Response.GetOrderListResponse>> GetOrdersList();
}