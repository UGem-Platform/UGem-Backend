using UGem.Repositories.Entity;

namespace UGem.Services.CustomerService;

public interface IService
{
    public Task<Response.GetCustomerDetailsResponse> GetProfile();
    
<<<<<<< feature/FixFunc

    
=======
    public Task ConfirmOrderReceived(Request.ConfirmOrderRequest request);
    
    public Task ConfirmOrderNotReceived(Request.ConfirmOrderRequest request);
    public Task<string> CreateCustomer(Request.RegisterCustomerRequest request);
>>>>>>> main
}
