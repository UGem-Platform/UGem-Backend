using UGem.Repositories.Entity;

namespace UGem.Services.OrderService;

public interface IService
{
    public Task<List<Response.GetOrderListResponse>> GetOrdersList( );
    public Task AcceptOrder(Guid orderId);
    public Task RejectOrder(Request.ReasonRejectRequest request);
    public Task CreateOrder(Request.CreateOrderRequest request);
    public Task<List<Response.OrderResponse>> GetOrderListFromCustomerId();
    public Task<List<Response.GetOrderDetailResponse>> GetOrderDetail(Guid orderId);
    public Task ConfirmOrderReceived(Request.ConfirmOrderRequest request);
    
    public Task ConfirmOrderNotReceived(Request.ConfirmOrderRequest request);

}