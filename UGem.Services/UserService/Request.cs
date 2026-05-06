namespace UGem.Services.UserService;

public class Request
{
    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        
    }
}