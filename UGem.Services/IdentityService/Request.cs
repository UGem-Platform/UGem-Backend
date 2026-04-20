namespace UGem.Service.Identity;

public class Request
{
    public class  CreateUserRequest
    {
        public required string  Email { get; set; }
        public required  string HashedPassword { get; set; }
        public required  string FullName { get; set; }
    }
}