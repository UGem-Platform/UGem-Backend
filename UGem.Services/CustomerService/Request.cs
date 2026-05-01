namespace UGem.Services.CustomerService;

public class Request
{
    public class ConfirmOrderRequest
    {
        public Guid OrderId { get; set; }
    }
    public class RegisterCustomerRequest
    {
        public required string Email { get; set; }
        public required string HashedPassword { get; set; }
        public required string PhoneNumber { get; set; }
        public required string FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public required string Role { get; set; }
    }
}