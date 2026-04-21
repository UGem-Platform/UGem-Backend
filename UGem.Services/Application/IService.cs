namespace UGem.Services.Application;

public interface IService
{
    public Task<string> CreateApplicationRequest(Request.CreateApplicationRequest request);
    public Task AcceptApplication(Guid id, Guid staffId);
    public Task<List<Response.GetApplicationForStaffResponse>> GetApplications(string? status = null);
   
}