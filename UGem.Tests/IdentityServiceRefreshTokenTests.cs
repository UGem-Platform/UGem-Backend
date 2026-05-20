using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using UGem.Repositories;
using UGem.Repositories.Entity;
using UGem.Services.IdentityService;
using UGem.Services.JwtService;
using IdentityService = UGem.Services.IdentityService.Service;
using JwtService = UGem.Services.JwtService.Service;

namespace UGem.Tests;

public class IdentityServiceRefreshTokenTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Login_IssuesRefreshTokenAndStoresOnlyHash()
    {
        await using var context = CreateContext();
        var user = SeedCustomer(context);
        var service = CreateService(context);

        var response = await service.Login(new Request.LoginRequest
        {
            Email = user.Email,
            Password = "123456"
        });

        var storedToken = await context.UserRefreshTokens.SingleAsync();
        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.NotEqual(response.RefreshToken, storedToken.TokenHash);
        Assert.Equal(user.Id, storedToken.UserId);
    }

    [Fact]
    public async Task RefreshToken_RequiresStoredRefreshTokenAndRotatesIt()
    {
        await using var context = CreateContext();
        var user = SeedCustomer(context);
        var service = CreateService(context, accessTokenMinutes: -1);

        var login = await service.Login(new Request.LoginRequest
        {
            Email = user.Email,
            Password = "123456"
        });

        var oldStoredToken = await context.UserRefreshTokens.SingleAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RefreshToken(new Request.RefreshTokenRequest
            {
                AccessToken = login.AccessToken,
                RefreshToken = "not-the-stored-refresh-token"
            }));

        var refreshed = await service.RefreshToken(new Request.RefreshTokenRequest
        {
            AccessToken = login.AccessToken,
            RefreshToken = login.RefreshToken
        });

        var storedTokens = await context.UserRefreshTokens.OrderBy(x => x.CreatedAt).ToListAsync();
        var reloadedOldToken = storedTokens.Single(x => x.Id == oldStoredToken.Id);
        Assert.NotNull(reloadedOldToken.RevokedAtUtc);
        Assert.Equal(2, storedTokens.Count);
        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);
    }

    private static User SeedCustomer(AppDbContext context)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "customer@test.local",
            FullName = "Test Customer",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            PhoneNumber = "0900000000",
            Role = "Customer",
            IsActive = true
        };

        context.Users.Add(user);
        context.Customers.Add(new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user
        });
        context.SaveChanges();
        return user;
    }

    private static IdentityService CreateService(AppDbContext context, int accessTokenMinutes = 15)
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = "UGem_Test_Secret_Key_For_Refresh_Token_Tests_2026",
            Issuer = "UGem.Tests",
            Audience = "UGem.Tests",
            ExpireMinutes = accessTokenMinutes,
            RefreshTokenExpireDays = 30
        });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GoogleAuth:ClientId"] = "test-client"
            })
            .Build();

        return new IdentityService(
            configuration,
            jwtOptions,
            new JwtService(jwtOptions),
            context,
            new Mock<UGem.Services.MailService.IService>().Object,
            new MemoryCache(new MemoryCacheOptions()));
    }
}
