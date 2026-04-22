namespace UGem.Services.OrderService;

public interface IService
{
    public Task CreateOrder(Request.CreateOrderRequest request);
}