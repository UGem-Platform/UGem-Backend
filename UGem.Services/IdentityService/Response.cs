namespace UGem.Services.IdentityService;

public class Response
{
    public class IdentityResponse
    {
        public string AccessToken { get; set; } = null!;
    }
    public class IdentityResponseGoogle
    {
        public string AccessToken { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? AvatarUrl { get; set; } 
        public bool IsNewUser { get; set; }
    }
}