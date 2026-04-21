namespace UGem.Services.Application;

public interface IService
{
    public Task<string> CreateApplicationRequest(Request.CreateApplicationRequest request);
}