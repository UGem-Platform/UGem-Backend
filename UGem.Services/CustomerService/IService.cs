namespace UGem.Services.CustomerService;

public interface IService
{
    public Task<List<Response.SearchUserByPhoneNumberResponse>> SearchUserByPhoneNumber(string? phoneNumber, int limit = 10);
    public Task<List<Response.SearchUserByPhoneNumberResponse>> SearchUserByEmail(string? email, int limit = 10);
}
