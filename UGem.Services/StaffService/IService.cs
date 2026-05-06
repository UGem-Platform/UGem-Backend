namespace UGem.Services.StaffService;

public interface IService
{
    Task ApproveApplication(Request.ApproveReviewerApplicationRequest request);

    Task RejectApplication(Request.RejectReviewerApplicationRequest request);

    public Task<Base.Response.PageResult<Response.ReviewerApplicationResponse>> GetReviewerApplications(
        string? searchTerm,
        int pageSize,
        int pageIndex);
}