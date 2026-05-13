using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.AffiliateLinkService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(
        AppDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

public async Task<Response.CreateAffiliateLinkResponse> CreateAffiliateLink(
    Request.CreateAffiliateLinkRequest request)
{
    var customerId = GetRequiredGuidClaim("CustomerId");

    var reviewer = await _dbContext.Reviewers
        .FirstOrDefaultAsync(x => x.CustomerId == customerId);

    if (reviewer == null)
        throw new Exception("Reviewer not found");

    var merchant = await _dbContext.Merchants
        .FirstOrDefaultAsync(x => x.Id == request.MerchantId);

    if (merchant == null)
        throw new Exception("Merchant not found");

    var existedAffiliateLink = await _dbContext.AffiliateLinks
        .FirstOrDefaultAsync(x =>
            x.ReviewerId == reviewer.Id &&
            x.MerchantId == merchant.Id);

    if (existedAffiliateLink != null)
    {
        return new Response.CreateAffiliateLinkResponse
        {
            AffiliateLinkId = existedAffiliateLink.Id,
            LinkCode = existedAffiliateLink.LinkCode,
            Url = $"https://u-gem.vercel.app/merchant/{merchant.Id}?ref={existedAffiliateLink.LinkCode}",
            ClickCount = existedAffiliateLink.ClickCount,
            IsActive = existedAffiliateLink.IsActive
        };
    }

    var linkCode =
        $"UGEM-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

    var affiliateLink = new AffiliateLink
    {
        Id = Guid.NewGuid(),
        ReviewerId = reviewer.Id,
        MerchantId = merchant.Id,
        LinkCode = linkCode,
        ClickCount = 0,
        OrderCount = 0,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    _dbContext.AffiliateLinks.Add(affiliateLink);

    await _dbContext.SaveChangesAsync();

    return new Response.CreateAffiliateLinkResponse
    {
        AffiliateLinkId = affiliateLink.Id,
        LinkCode = affiliateLink.LinkCode,
        Url = $"https://u-gem.vercel.app/merchant/{merchant.Id}?ref={affiliateLink.LinkCode}",
        ClickCount = affiliateLink.ClickCount,
        IsActive = affiliateLink.IsActive
    };
}

    private Guid GetRequiredGuidClaim(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == claimType)?.Value;

        if (string.IsNullOrWhiteSpace(value))
            throw new UnauthorizedAccessException($"{claimType} not found");

        return Guid.Parse(value);
    }
}