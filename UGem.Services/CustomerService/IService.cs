using UGem.Repositories.Entity;

namespace UGem.Services.CustomerService;

public interface IService
{
    public Task<Response.GetCustomerDetailsResponse> GetProfile();
    
    public Task<string> CreateCustomer(Request.RegisterCustomerRequest request);

}
