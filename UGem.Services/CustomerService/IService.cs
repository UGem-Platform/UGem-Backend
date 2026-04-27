using UGem.Repositories.Entity;

namespace UGem.Services.CustomerService;

public interface IService
{
    public Task<Response.GetCustomerDetailsResponse> GetProfile();
    
    public Task ConfirmOrderReceived(Request.ConfirmOrderRequest request);
    
    public Task ConfirmOrderNotReceived(Request.ConfirmOrderRequest request);
    
}
