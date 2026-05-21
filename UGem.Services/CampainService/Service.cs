using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.CampainService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Response.CampaignResponse>> GetCampaigns()
    {
        return await _dbContext.Campaigns
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new Response.CampaignResponse
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Description = x.Description,
                DiscountValue = x.DiscountValue,
                IsPercentage = x.IsPercentage,
                MinOrderAmount = x.MinOrderAmount,
                MaxDiscountAmount = x.MaxDiscountAmount,
                Quantity = x.Quantity,
                UsedCount = x.UsedCount,
                MaxUsagePerUser = x.MaxUsagePerUser,
                IsGlobal = x.IsGlobal,
                IsNewUserOnly = x.IsNewUserOnly,
                IsActive = x.IsActive,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                MerchantId = x.MerchantId
            })
            .ToListAsync();
    }

    public async Task<Response.CampaignResponse?> GetCampaignById(Guid id)
    {
        return await _dbContext.Campaigns
            .Where(x => x.Id == id)
            .Select(x => new Response.CampaignResponse
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Description = x.Description,
                DiscountValue = x.DiscountValue,
                IsPercentage = x.IsPercentage,
                MinOrderAmount = x.MinOrderAmount,
                MaxDiscountAmount = x.MaxDiscountAmount,
                Quantity = x.Quantity,
                UsedCount = x.UsedCount,
                MaxUsagePerUser = x.MaxUsagePerUser,
                IsGlobal = x.IsGlobal,
                IsNewUserOnly = x.IsNewUserOnly,
                IsActive = x.IsActive,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                MerchantId = x.MerchantId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<string> CreateCampaign(
        Request.CreateCampaignRequest request,
        Guid userId)
    {
        var user = await _dbContext.Users
            .Include(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var existCode = await _dbContext.Campaigns
            .AnyAsync(x => x.Code == request.Code);

        if (existCode)
        {
            throw new Exception("Campaign code already exists");
        }

        if (request.EndDate <= request.StartDate)
        {
            throw new Exception(
                "EndDate must be greater than StartDate");
        }

        if (request.Quantity <= 0)
        {
            throw new Exception(
                "Quantity must be greater than 0");
        }

        if (request.MaxUsagePerUser <= 0)
        {
            throw new Exception(
                "MaxUsagePerUser must be greater than 0");
        }

        if (request.DiscountValue <= 0)
        {
            throw new Exception(
                "DiscountValue must be greater than 0");
        }

        // merchant không được tạo global voucher
        if (user.Merchant != null && request.IsGlobal)
        {
            throw new Exception(
                "Merchant cannot create global campaign");
        }

        var campaign = new Campaign
        {
            Code = request.Code.ToUpper(),

            Title = request.Title,

            Description = request.Description,

            DiscountValue = request.DiscountValue,

            IsPercentage = request.IsPercentage,

            MinOrderAmount = request.MinOrderAmount,

            MaxDiscountAmount = request.MaxDiscountAmount,

            Quantity = request.Quantity,

            UsedCount = 0,

            MaxUsagePerUser = request.MaxUsagePerUser,

            IsGlobal = request.IsGlobal,

            IsNewUserOnly = request.IsNewUserOnly,

            IsActive = true,

            StartDate = request.StartDate,

            EndDate = request.EndDate,

            CreatedByUserId = userId,

            MerchantId = request.IsGlobal
                ? null
                : user.Merchant!.Id
        };

        _dbContext.Campaigns.Add(campaign);

        await _dbContext.SaveChangesAsync();

        return "Create campaign successfully";
    }

    public async Task<string> UpdateCampaign(
        Request.UpdateCampaignRequest request,
        Guid userId)
    {
        var user = await _dbContext.Users
            .Include(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var campaign = await _dbContext.Campaigns
            .FirstOrDefaultAsync(x => x.Id == request.Id);

        if (campaign == null)
        {
            throw new Exception("Campaign not found");
        }

        // merchant không được sửa voucher global
        if (user.Merchant != null && campaign.IsGlobal)
        {
            throw new Exception(
                "Merchant cannot update global campaign");
        }

        // merchant chỉ được sửa voucher của chính họ
        if (user.Merchant != null &&
            campaign.MerchantId != user.Merchant.Id)
        {
            throw new Exception(
                "You cannot update another merchant campaign");
        }

        // merchant không được convert thành global
        if (user.Merchant != null && request.IsGlobal)
        {
            throw new Exception(
                "Merchant cannot create global campaign");
        }

        if (request.EndDate <= request.StartDate)
        {
            throw new Exception(
                "EndDate must be greater than StartDate");
        }

        if (request.Quantity <= 0)
        {
            throw new Exception(
                "Quantity must be greater than 0");
        }

        if (request.MaxUsagePerUser <= 0)
        {
            throw new Exception(
                "MaxUsagePerUser must be greater than 0");
        }

        if (request.DiscountValue <= 0)
        {
            throw new Exception(
                "DiscountValue must be greater than 0");
        }

        campaign.Code = request.Code.ToUpper();

        campaign.Title = request.Title;

        campaign.Description = request.Description;

        campaign.DiscountValue = request.DiscountValue;

        campaign.IsPercentage = request.IsPercentage;

        campaign.MinOrderAmount = request.MinOrderAmount;

        campaign.MaxDiscountAmount = request.MaxDiscountAmount;

        campaign.Quantity = request.Quantity;

        campaign.MaxUsagePerUser = request.MaxUsagePerUser;

        campaign.IsGlobal = request.IsGlobal;

        campaign.IsNewUserOnly = request.IsNewUserOnly;

        campaign.IsActive = request.IsActive;

        campaign.StartDate = request.StartDate;

        campaign.EndDate = request.EndDate;

        await _dbContext.SaveChangesAsync();

        return "Update campaign successfully";
    }

    public async Task<string> DeleteCampaign(
        Guid id,
        Guid userId)
    {
        var user = await _dbContext.Users
            .Include(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var campaign = await _dbContext.Campaigns
            .FirstOrDefaultAsync(x => x.Id == id);

        if (campaign == null)
        {
            throw new Exception("Campaign not found");
        }

        // merchant không được xóa global voucher
        if (user.Merchant != null && campaign.IsGlobal)
        {
            throw new Exception(
                "Merchant cannot delete global campaign");
        }

        // merchant chỉ được xóa voucher của chính họ
        if (user.Merchant != null &&
            campaign.MerchantId != user.Merchant.Id)
        {
            throw new Exception(
                "You cannot delete another merchant campaign");
        }

        _dbContext.Campaigns.Remove(campaign);

        await _dbContext.SaveChangesAsync();

        return "Delete campaign successfully";
    }

    public async Task<Response.ApplyCampaignResponse> ApplyCampaign(Request.ApplyCampaignRequest request, Guid userId)
{
    var campaign = await _dbContext.Campaigns.FirstOrDefaultAsync(x => x.Code == request.Code.ToUpper());

    if (campaign == null)
    {
        throw new Exception("Campaign not found");
    }

    if (!campaign.IsActive)
    {
        throw new Exception("Campaign inactive");
    }

    var now = DateTimeOffset.UtcNow;

    if (now < campaign.StartDate ||
        now > campaign.EndDate)
    {
        throw new Exception("Campaign expired");
    }

    if (campaign.UsedCount >= campaign.Quantity)
    {
        throw new Exception(
            "Campaign out of quantity");
    }

    // voucher merchant
    if (!campaign.IsGlobal &&
        campaign.MerchantId != request.MerchantId)
    {
        throw new Exception(
            "Campaign not valid for this merchant");
    }

    // min order
    if (campaign.MinOrderAmount.HasValue &&
        request.TotalAmount <
        campaign.MinOrderAmount.Value)
    {
        throw new Exception(
            $"Minimum order is {campaign.MinOrderAmount.Value}");
    }

    // check usage
    var usage = await _dbContext.UserCampaignUsages
        .FirstOrDefaultAsync(x =>
            x.UserId == userId &&
            x.CampaignId == campaign.Id);

    if (usage != null &&
        usage.UsedCount >=
        campaign.MaxUsagePerUser)
    {
        throw new Exception(
            "You already used this campaign");
    }

    decimal discountAmount;

    if (campaign.IsPercentage)
    {
        discountAmount =
            request.TotalAmount *
            campaign.DiscountValue / 100;

        if (campaign.MaxDiscountAmount.HasValue)
        {
            discountAmount = Math.Min(
                discountAmount,
                campaign.MaxDiscountAmount.Value);
        }
    }
    else
    {
        discountAmount =
            campaign.DiscountValue;
    }

    // chống âm bill
    discountAmount = Math.Min(
        discountAmount,
        request.TotalAmount);

    var finalAmount =
        request.TotalAmount -
        discountAmount;

    return new Response.ApplyCampaignResponse
    {
        CampaignId = campaign.Id,

        Code = campaign.Code,

        Title = campaign.Title,

        IsPercentage =
            campaign.IsPercentage,

        DiscountValue =
            campaign.DiscountValue,

        MaxDiscountAmount =
            campaign.MaxDiscountAmount ?? 0,

        MinOrderAmount =
            campaign.MinOrderAmount ?? 0,

        DiscountAmount =
            discountAmount,

        FinalAmount =
            finalAmount,

        Message =
            "Apply campaign successfully"
    };
}
    public async Task<Response.ApplyCampaignResponse?> GetBestCampaign(Request.GetBestCampaignRequest request, Guid userId)
{
    var now = DateTimeOffset.UtcNow;

    var campaigns = await _dbContext.Campaigns
        .Where(x =>
            x.IsActive &&
            x.StartDate <= now &&
            x.EndDate >= now &&
            x.UsedCount < x.Quantity &&
            (
                x.IsGlobal ||
                x.MerchantId == request.MerchantId
            ))
        .ToListAsync();

    Response.ApplyCampaignResponse? bestCampaign = null;

    decimal bestDiscount = 0;

    foreach (var campaign in campaigns)
    {
        // minimum order
        if (campaign.MinOrderAmount.HasValue &&
            request.TotalAmount < campaign.MinOrderAmount.Value)
        {
            continue;
        }

        // check user usage
        var usage = await _dbContext.UserCampaignUsages
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.CampaignId == campaign.Id);

        if (usage != null &&
            usage.UsedCount >= campaign.MaxUsagePerUser)
        {
            continue;
        }

        decimal discount;

        // percentage
        if (campaign.IsPercentage)
        {
            discount =
                request.TotalAmount *
                campaign.DiscountValue / 100;

            // max cap
            if (campaign.MaxDiscountAmount.HasValue)
            {
                discount = Math.Min(
                    discount,
                    campaign.MaxDiscountAmount.Value);
            }
        }
        else
        {
            discount = campaign.DiscountValue;
        }

        // KHÔNG CHO discount > total
        discount = Math.Min(discount, request.TotalAmount);

        var finalAmount = request.TotalAmount - discount;

        if (discount > bestDiscount)
        {
            bestDiscount = discount;

            bestCampaign =
                new Response.ApplyCampaignResponse
                {
                    CampaignId = campaign.Id,

                    Code = campaign.Code,

                    Title = campaign.Title,

                    IsPercentage =
                        campaign.IsPercentage,

                    DiscountValue =
                        campaign.DiscountValue,

                    DiscountAmount = discount,

                    FinalAmount = finalAmount,

                    MaxDiscountAmount =
                        campaign.MaxDiscountAmount ?? 0,

                    MinOrderAmount =
                        campaign.MinOrderAmount ?? 0,

                    Message = "Best campaign found"
                };
        }
    }

    return bestCampaign;
}
    public async Task ConfirmCampaignUsage(
        Request.ConfirmCampaignUsageRequest request,
        Guid userId)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x =>
                x.Id == request.OrderId);

        if (order == null)
        {
            throw new Exception("Order not found");
        }

        var campaign = await _dbContext.Campaigns
            .FirstOrDefaultAsync(x =>
                x.Id == request.CampaignId);

        if (campaign == null)
        {
            throw new Exception("Campaign not found");
        }

        if (order.CampaignId != null)
        {
            throw new Exception(
                "Campaign already applied");
        }

        var usage =
            await _dbContext.UserCampaignUsages
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.CampaignId == campaign.Id);

        if (usage == null)
        {
            usage = new UserCampaignUsage
            {
                Id = Guid.NewGuid(),

                UserId = userId,

                CampaignId = campaign.Id,

                UsedCount = 1,

                LastUsedAt =
                    DateTimeOffset.UtcNow,

                CreatedAt =
                    DateTimeOffset.UtcNow
            };

            _dbContext.UserCampaignUsages
                .Add(usage);
        }
        else
        {
            usage.UsedCount += 1;

            usage.LastUsedAt =
                DateTimeOffset.UtcNow;

            usage.UpdatedAt =
                DateTimeOffset.UtcNow;
        }

        campaign.UsedCount += 1;

        campaign.UpdatedAt =
            DateTimeOffset.UtcNow;

        order.CampaignId = campaign.Id;

        order.AppliedCampaignCode =
            campaign.Code;

        order.UpdatedAt =
            DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
}