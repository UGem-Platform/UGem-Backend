namespace UGem.Services.CustomerService;

public class Response
{
    public class GetCustomerDetailsResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
    public class SearchUserByPhoneNumberResponse
    {
        public Guid UserId { get; set; }
        public Guid CustomerId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; } 
        public required string Role { get; set; } 
        public string? AvatarUrl { get; set; }
    }
}
