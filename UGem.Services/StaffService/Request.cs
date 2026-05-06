namespace UGem.Services.StaffService;

public class Request
{
    public class ApproveReviewerApplicationRequest
    {
        public Guid ApplicationId { get; set; }
    }

    public class RejectReviewerApplicationRequest
    {
        public Guid ApplicationId { get; set; }
        public string Reason { get; set; } = null!;
    }
   

}