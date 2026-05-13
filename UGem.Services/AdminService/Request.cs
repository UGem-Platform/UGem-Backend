namespace UGem.Services.AdminService;

public class Request
{
    public class CreateStaffRequest
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string Password { get; set; }
        public required string PhoneNumber { get; set; }
    }
}