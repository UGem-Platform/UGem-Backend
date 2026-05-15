namespace UGem.Services.AdminService;

public interface IService
{
    public Task<Base.Response.PageResult<Response.StaffResponse>> GetAllStaffForAdmin(string? searchTerm, int pageSize,
        int pageIndex);
    public Task CreateStaff(Request.CreateStaffRequest request);
    public Task DeleteStaff(Guid staffId);
    
    public Task<Response.DashboardResponse> GetDashboard();
    public Task<List<Response.MerchantRevenueResponse>> GetMerchantRevenues(string? searchTerm, int pageIndex, int pageSize);
    public Task<Response.MerchantDetailResponse> GetMerchantDetail(Guid merchantId, string periodType);
}