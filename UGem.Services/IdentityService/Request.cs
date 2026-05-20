namespace UGem.Services.IdentityService;

public class Request
{
    public class CreateUserRequest
    {
        public required string Email { get; set; }
        public required string HashedPassword { get; set; }
        public required string FullName { get; set; }
    }

    public class RegisterUserRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string PhoneNumber { get; set; }
        public required string FullName { get; set; }
        public required string Role { get; set; }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
    public class GoogleLoginRequest
    {
        public required string IdToken { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public required string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmNewPassword { get; set; }
    }
}