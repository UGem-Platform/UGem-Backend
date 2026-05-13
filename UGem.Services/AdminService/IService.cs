namespace UGem.Services.AdminService;

public interface IService
{
    public Task<Base.Response.PageResult<Response.StaffResponse>> GetAllStaffForAdmin(string? searchTerm, int pageSize,
        int pageIndex);
    public Task CreateStaff(Request.CreateStaffRequest request);
    public Task DeleteStaff(Guid staffId);
    public Task<Response.DashboardResponse> GetDashboard();
}