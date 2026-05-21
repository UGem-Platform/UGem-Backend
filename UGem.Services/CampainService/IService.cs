namespace UGem.Services.CampainService;

public interface IService
{
    Task<List<Response.CampaignResponse>> GetCampaigns();

    Task<Response.CampaignResponse?> GetCampaignById(Guid id);

    Task<string> CreateCampaign(Request.CreateCampaignRequest request, Guid userId);

    Task<string> UpdateCampaign(Request.UpdateCampaignRequest request, Guid userId);

    Task<string> DeleteCampaign(Guid id, Guid userId);
    Task<Response.ApplyCampaignResponse> ApplyCampaign(Request.ApplyCampaignRequest request, Guid userId);

    Task ConfirmCampaignUsage(Request.ConfirmCampaignUsageRequest request, Guid userId);
    Task<Response.ApplyCampaignResponse?>
        GetBestCampaign(Request.GetBestCampaignRequest request, Guid userId);
}