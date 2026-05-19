namespace UGem.Services.AffiliateLinkService;

public interface IService
{
    public Task<Response.CreateAffiliateLinkResponse> CreateAffiliateLink(
        Request.CreateAffiliateLinkRequest request);

    Task<string> TrackClickAndGetRedirectUrl(string linkCode);
}