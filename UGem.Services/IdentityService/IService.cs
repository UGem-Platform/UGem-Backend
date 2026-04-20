namespace UGem.Service.Identity;

public interface IService
{
    public Task<Response.IdentityResponse> Login(string phoneNumber, string password);

}