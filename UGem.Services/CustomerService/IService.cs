namespace UGem.Services.CustomerService;

public interface IService
{
    public Task ConfirmOrderReceived(Request.ConfirmOrderRequest request);
    
    public Task ConfirmOrderNotReceived(Request.ConfirmOrderRequest request);
}
