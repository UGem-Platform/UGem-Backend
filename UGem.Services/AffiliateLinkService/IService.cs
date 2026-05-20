namespace UGem.Services.AffiliateLinkService;

public interface IService
{
    public Task<Response.CreateAffiliateLinkResponse> CreateAffiliateLink(
        Request.CreateAffiliateLinkRequest request);

    Task<Response.ReviewerAffiliateEarningsResponse> GetReviewerAffiliateEarnings();

    Task<string> TrackClickAndGetRedirectUrl(string linkCode);
}