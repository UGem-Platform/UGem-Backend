using System.ComponentModel;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UGem.Repositories;
using UGem.Services.JwtService;
using UGem.Services.MailService;
using Google.Apis.Auth;
using UGem.Repositories.Entity;

namespace UGem.Services.IdentityService;

public class Service : IService
{
    private readonly JwtService.IService _jwtService;
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOption = new();
    private readonly MailService.IService _mailService;
private readonly IConfiguration _configuration;
    public Service(IConfiguration configuration, JwtService.IService jwtService, AppDbContext dbContext,
        MailService.IService mailService)
    {
        _configuration = configuration;
        _jwtService = jwtService;
        _dbContext = dbContext;
        _mailService = mailService;
        configuration.GetSection(nameof(JwtOptions)).Bind(_jwtOption);
    }

    public async Task<Response.IdentityResponse> Login(Request.LoginRequest request)
    {
        var user = await _dbContext.Users
            .Include(x => x.Customer)
            .Include(x => x.Staff)
            .Include(x => x.Merchant)
            .Include(x => x.Admin)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid password");
        }


        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Email", user.Email),
            new Claim(type: "Name", user.FullName),
            new Claim("Role", user.Role),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Expired,
                DateTimeOffset.UtcNow.AddMinutes(_jwtOption.ExpireMinutes).ToString()),
        };

        var customer = await _dbContext.Customers.FirstOrDefaultAsync(u => u.UserId == user.Id);
        if (customer != null)
        {
            claims.Add(new Claim("CustomerId", customer.Id.ToString()));
        }

        var merchant = await _dbContext.Merchants.FirstOrDefaultAsync(u => u.UserId == user.Id);
        if (merchant != null)
        {
            claims.Add(new Claim("MerchantId", merchant.Id.ToString()));
        }

        var staff = await _dbContext.Staffs.FirstOrDefaultAsync(u => u.UserId == user.Id);
        if (staff != null)
        {
            claims.Add(new Claim("StaffId", staff.Id.ToString()));
        }

        var admin = await _dbContext.Admins.FirstOrDefaultAsync(u => u.UserId == user.Id);
        if (admin != null)
        {
            claims.Add(new Claim("AdminId", admin.Id.ToString()));
        }

        var token = _jwtService.GenerateAccessToken(claims);

        var result = new Response.IdentityResponse()
        {
            AccessToken = token
        };

        return result;
    }

    public async Task<string> Register(Request.RegisterUserRequest request)
    {
        if (!System.Net.Mail.MailAddress.TryCreate(request.Email, out _))
            throw new Exception("Invalid email format");
        var isExistUser = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
        if (isExistUser)
        {
            throw new Exception("User Already Exist with this mail");
        }

        var allowedRoles = new[] { "Customer", "Merchant" };
        if (!allowedRoles.Contains(request.Role))
        {
            throw new Exception("Invalid role. Only 'Customer' or 'Merchant' are allowed");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = hashedPassword,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        if (request.Role == "Customer")
        {
            var customer = new Customer
            {
                UserId = user.Id,
            };
            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await _mailService.SendMail(new MailContext()
                {
                    To = request.Email,
                    Subject = "Welcome to UGem!",
                    Body = $"Dear {request.FullName},\n\n" +
                           "Thank you for registering as a Customer on UGem."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send mail failed: {ex.Message}");
            }
        });
        return "Register Successfully";
    }

    public async Task<Response.IdentityResponseGoogle> GooleLogin(Request.GoogleLoginRequest request)
{
    var clinetId = _configuration["GoogleAuth:ClientId"];
    if (string.IsNullOrWhiteSpace(clinetId))
        throw new InvalidAsynchronousStateException("GoogleAuth:ClientId is not configured.");

    GoogleJsonWebSignature.Payload payload;
    try
    {
        payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clinetId },
            });
    }
    catch
    {
        throw new UnauthorizedAccessException("Invalid Google token");
    }

    var email = payload.Email?.Trim().ToLower();
    if (string.IsNullOrWhiteSpace(email))
        throw new UnauthorizedAccessException("Google token has no email");

    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

    if (user == null)
    {
        var allowedRoles = new[] { "Customer", "Merchant" };
        if (string.IsNullOrWhiteSpace(request.Role) || !allowedRoles.Contains(request.Role))
            throw new Exception("Invalid role. Only 'Customer' or 'Merchant' are allowed");

        user = new User
        {
            Email = email,
            PasswordHash = "",
            PhoneNumber = "",
            FullName = payload.Name,
            AvatarUrl = payload.Picture,
            Role = request.Role
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        if (request.Role == "Customer")
        {
            _dbContext.Customers.Add(new Customer { UserId = user.Id });
            await _dbContext.SaveChangesAsync();
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await _mailService.SendMail(new MailContext()
                {
                    To = user.Email,
                    Subject = "Welcome to UGem!",
                    Body = $"Dear {user.FullName},\n\nThank you for registering on UGem."
                });
            }
            catch (Exception ex) { Console.WriteLine($"Send mail failed: {ex.Message}"); }
        });
    }

    var token = await BuildTokenAsync(user);
    return new Response.IdentityResponseGoogle
    {
        AccessToken = token,
        FullName = user.FullName,
        Role = user.Role,
        AvatarUrl = user.AvatarUrl
    };
}
    private async Task<string> BuildTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Email", user.Email),
            new Claim("Name", user.FullName),
            new Claim("Role", user.Role),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Expired,
                DateTimeOffset.UtcNow.AddMinutes(_jwtOption.ExpireMinutes).ToString()),
        };

        var customer = await _dbContext.Customers.FirstOrDefaultAsync(u => u.UserId == user.Id);
        if (customer != null) claims.Add(new Claim("CustomerId", customer.Id.ToString()));

        var merchant = await _dbContext.Merchants.FirstOrDefaultAsync(u => u.UserId == user.Id);
        if (merchant != null) claims.Add(new Claim("MerchantId", merchant.Id.ToString()));

        var staff = await _dbContext.Staffs.FirstOrDefaultAsync(u => u.UserId == user.Id);
        if (staff != null) claims.Add(new Claim("StaffId", staff.Id.ToString()));

        var admin = await _dbContext.Admins.FirstOrDefaultAsync(u => u.UserId == user.Id);
        if (admin != null) claims.Add(new Claim("AdminId", admin.Id.ToString()));

        return _jwtService.GenerateAccessToken(claims);
    } 
    
}