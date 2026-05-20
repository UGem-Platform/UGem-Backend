namespace UGem.Services.CampainService;

public interface IService
{
    Task<List<Response.CampaignResponse>> GetCampaigns();

    Task<Response.CampaignResponse?> GetCampaignById(Guid id);

    Task<string> CreateCampaign(
        Request.CreateCampaignRequest request,
        Guid userId);

    Task<string> UpdateCampaign(
        Request.UpdateCampaignRequest request,
        Guid userId);

    Task<string> DeleteCampaign(
        Guid id,
        Guid userId);
}