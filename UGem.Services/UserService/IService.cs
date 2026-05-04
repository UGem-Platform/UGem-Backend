namespace UGem.Services.UserService;

public interface IService
{
    public Task<Response.GetCustomerDetailsResponse> GetProfile();
    
    public Task UpdateProfile(Request.UpdateProfileRequest request);


}
