namespace UGem.Services.CustomerService;

public interface IService
{
    public Task<List<Response.SearchUserByPhoneNumberResponse>> SearchUserByPhoneNumber(string? phoneNumber, int limit = 10);
}
