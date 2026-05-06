namespace UGem.Services.ReviewerApplicationService;

public interface IService
{
    public Task CreateReviewerApplication(Request.ReviewerApplicationRequest request);
    
    public Task UpdateReviewerApplication(Request.UpdateReviewerApplicationRequest request);

}