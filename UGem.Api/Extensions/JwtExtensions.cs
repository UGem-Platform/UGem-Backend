using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UGem.Services.JwtService;

namespace UGem.Api.Extensions;

public static class JwtExtensions
{
    public const string AdminPolicy = "AdminPolicy";
    public const string ReviewerPolicy = "ReviewerPolicy";
    public const string CustomerPolicy = "CustomerPolicy";
    public const string StaffPolicy = "StaffPolicy";
    public const string AdminAndStaffPolicy = "AdminAndStaffPolicy";
    public const string MerchantPolicy = "MerchantPolicy";
    public const string MerchantApplicantPolicy = "MerchantApplicantPolicy";
    public const string MerchantAndCustomer = "MerchantAndCustomer";


    public static void AddJwtServices(this IServiceCollection services, IConfiguration configuration)
    {
        JwtOptions jwtOption = new JwtOptions();
        configuration.GetSection(nameof(JwtOptions)).Bind(jwtOption);
        var key = Encoding.UTF8.GetBytes(jwtOption.SecretKey);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOption.Issuer,
                    ValidAudience = jwtOption.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AdminPolicy, policy =>
                policy.RequireRole("Admin"));
            // [Authorize(Policy = JwtExtensions.AdminPolicy)]

            options.AddPolicy(ReviewerPolicy, policy =>
                policy.RequireRole("Reviewer"));
            // [Authorize(Policy = JwtExtensions.ReviewerPolicy)]

            options.AddPolicy(CustomerPolicy, policy =>
                policy.RequireRole("Customer"));
            // [Authorize(Policy = JwtExtensions.CustomerPolicy)]

            options.AddPolicy(StaffPolicy, policy =>
                policy.RequireRole("Staff"));

            // [Authorize(Policy = JwtExtensions.StaffPolicy)]

            options.AddPolicy(AdminAndStaffPolicy, policy =>
                policy.RequireRole("Admin", "Staff"));

            // [Authorize(Policy = JwtExtensions.AdminAndStaffPolicy)]

            //// [Authorize(Policy = JwtExtensions.MerchantPolicy)]
            options.AddPolicy(MerchantPolicy, policy =>
                policy.RequireRole("Merchant"));
            options.AddPolicy(MerchantApplicantPolicy, policy =>
                policy.RequireRole("Customer", "Merchant"));
            options.AddPolicy(MerchantAndCustomer, policy =>
                policy.RequireRole("Customer", "Merchant"));
        });
    }
}