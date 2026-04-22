namespace UGem.Services.Application;

public interface IService
{
    public Task AcceptApplication(Guid id);

    public Task<List<Response.GetApplicationForStaffResponse>> GetApplications();

    public Task<string> CreateApplicationRequest(Request.ApplicationRequest request);

    public Task<string> RejectApplication(Request.RejectApplicationRequest request);

    public Task<string> EditApplicationAfterReject(Request.UpdateApplicationRequest request);
}