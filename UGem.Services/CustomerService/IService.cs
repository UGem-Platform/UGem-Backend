using UGem.Repositories.Entity;

namespace UGem.Services.CustomerService;

public interface IService
{
    public Task<List<Response.SearchUserByEmailResponse>> SearchUserByEmail(string? email, int limit = 10);

}
