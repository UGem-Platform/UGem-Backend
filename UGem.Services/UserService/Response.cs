namespace UGem.Services.UserService;

public class Response
{
    public class GetCustomerDetailsResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; } 
        public required string PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public required string Role { get; set; } 
    }

}