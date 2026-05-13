namespace UGem.Services.StaffService;

public class Request
{
    public class ApproveReviewerApplicationRequest
    {
        public Guid ReviewerApplicationId { get; set; }
    }

    public class RejectReviewerApplicationRequest
    {
        public Guid ReviewerApplicationId { get; set; }
        public string Reason { get; set; } = null!;
    }
   

}