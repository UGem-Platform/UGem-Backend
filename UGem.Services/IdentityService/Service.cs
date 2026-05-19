using System.ComponentModel;
using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using UGem.Repositories;
using UGem.Repositories.Entity;
using UGem.Services.JwtService;
using UGem.Services.MailService;

namespace UGem.Services.IdentityService;

public class Service : IService
{
    private readonly JwtService.IService _jwtService;
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOption;
    private readonly MailService.IService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    public Service(
        IConfiguration configuration,
        IOptions<JwtOptions> jwtOptions,
        JwtService.IService jwtService,
        AppDbContext dbContext,
        MailService.IService mailService, IMemoryCache memoryCache)
    {
        _configuration = configuration;
        _jwtOption = jwtOptions.Value;
        _jwtService = jwtService;
        _dbContext = dbContext;
        _mailService = mailService;
        _cache = memoryCache;
    }

    public async Task<Response.IdentityResponse> Login(Request.LoginRequest request)
    {
        var user = await _dbContext.Users
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
            new("UserId", user.Id.ToString()),
            new("Email", user.Email),
            new Claim(type: "Name", user.FullName),
            new("Role", user.Role),
            new(ClaimTypes.Role, user.Role),
            new(ClaimTypes.Expired, DateTimeOffset.UtcNow.AddMinutes(_jwtOption.ExpireMinutes).ToString()),
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
        return new Response.IdentityResponse
        {
            AccessToken = token
        };
    }

    public async Task<string> Register(Request.RegisterUserRequest request)
    {
        var isExistPhoneNumber = await _dbContext.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (isExistPhoneNumber)
        {
            throw new Exception("User Already Exist with this phone number");
        }
        if (!System.Net.Mail.MailAddress.TryCreate(request.Email, out _))
        {
            throw new Exception("Invalid email format");
        }

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
                await _mailService.SendMail(new MailContext
                {
                    To = request.Email,
                    Subject = "Welcome to UGem!",
                    Body = $"Dear {request.FullName},\n\nThank you for registering as a Customer on UGem."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send mail failed: {ex.Message}");
            }
        });

        return "Register Successfully";
    }

    public async Task<Response.IdentityResponse> RefreshToken(Request.RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedAccessException("Invalid token");
        }

        var principal = _jwtService.ValidateToken(request.RefreshToken, validateLifetime: false);
        var userIdValue = principal?.FindFirst("UserId")?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid token");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return new Response.IdentityResponse
        {
            AccessToken = await BuildTokenAsync(user)
        };
    }

    public async Task<Response.IdentityResponseGoogle> GooleLogin(Request.GoogleLoginRequest request)
    {
        var clinetId = _configuration["GoogleAuth:ClientId"];
        if (string.IsNullOrWhiteSpace(clinetId))
        {
            throw new InvalidAsynchronousStateException("GoogleAuth:ClientId is not configured.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [clinetId],
                });
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAccessException("Invalid Google token", ex);
        }

        var email = payload.Email?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedAccessException("Google token has no email");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new User
            {
                Email = email,
                PasswordHash = string.Empty,
                PhoneNumber = string.Empty,
                FullName = payload.Name,
                AvatarUrl = payload.Picture,
                Role = "Customer"
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _dbContext.Customers.Add(new Customer { UserId = user.Id });
            await _dbContext.SaveChangesAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    await _mailService.SendMail(new MailContext
                    {
                        To = user.Email,
                        Subject = "Welcome to UGem!",
                        Body = $"Dear {user.FullName},\n\nThank you for registering on UGem."
                    });
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Send mail failed: {ex.Message}");
                }
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

    public async Task ForgotPassword(Request.ForgotPasswordRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var otp = Random.Shared.Next(100000, 999999).ToString();
        _cache.Set($"reset:{request.Email}", otp, TimeSpan.FromMinutes(10));

        await _mailService.SendMail(new MailContext
        {
            To = request.Email,
            Subject = "UGem - Mã xác nhận đặt lại mật khẩu",
            Body = $"Xin chào {user.FullName},\n\n" +
                   $"Mã xác nhận đặt lại mật khẩu của bạn là: {otp}\n\n" +
                   $"Mã có hiệu lực trong 10 phút.\n\n" +
                   $"Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này."
        });
    }

    public async Task ResetPassword(Request.ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new InvalidAsynchronousStateException("Passwords don't match");
        }
        if (!_cache.TryGetValue($"reset:{request.Email}", out string? cachedOtp))
        {
            throw new InvalidOperationException("OTP expired or not found. Please request a new one.");
        }

        if (cachedOtp != request.Token)
        {
            throw new InvalidOperationException("Invalid OTP token");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        _dbContext.Notifications.Add(new Notification
        {
            Message = "Mật khẩu của bạn đã được đặt lại thành công.",
            Title = "Mật khẩu đã được đặt lại",
            Type = "Security",
            UserId = user.Id,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync();
        _cache.Remove($"reset:{request.Email}");
    }

    private async Task<string> BuildTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new("UserId", user.Id.ToString()),
            new("Email", user.Email),
            new("Name", user.FullName),
            new("Role", user.Role),
            new(ClaimTypes.Role, user.Role),
            new(ClaimTypes.Expired, DateTimeOffset.UtcNow.AddMinutes(_jwtOption.ExpireMinutes).ToString()),
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

        return _jwtService.GenerateAccessToken(claims);
    }
}
