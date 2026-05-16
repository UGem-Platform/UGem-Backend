using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.CheckInService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task<byte[]> GenerateQrCode(Request.GenerateQrCodeRequest request)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == request.OrderId);

        if (order == null)
            throw new KeyNotFoundException("Order not found");
        
        if (order == null)
            throw new KeyNotFoundException("Order not found or not yours");

        if (order.OrderType != "Offline")
            throw new InvalidOperationException("Check-in QR can only be generated for offline orders");
        
        var qrText = $"https://u-gem.vercel.app/check-in?orderId={request.OrderId}";
        
        using var qrGenerator = new QRCodeGenerator();
        
        var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
        
        var qrCode = new PngByteQRCode(qrCodeData);
        
        return qrCode.GetGraphic(20);
    }

    public async Task CreateCheckIn(Guid customerId, Guid merchantId)
    {
        var checkIn = new CheckIn()
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            MerchantId = merchantId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.CheckIns.Add(checkIn);

        var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customerId);
        if (customer != null)
        {
            customer.TotalCheckIns += 1;
        } 
        await _dbContext.SaveChangesAsync();
    }


    public async Task CreateCheckInForQr(Request.CreateCheckInForQr request)
    {
        var customerId = _httpContext.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "CustomerId")?.Value;

        if (string.IsNullOrEmpty(customerId))
            throw new UnauthorizedAccessException("CustomerId not found");

        var customerIdGuid = Guid.Parse(customerId);

        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.Food)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId );

        if (order == null)
            throw new KeyNotFoundException("Order not found");
        
        
        if (order.OrderType != "Offline")
            throw new InvalidOperationException("Check-in QR is only available for offline orders");

        
        if (order.CustomerId != Guid.Empty)
        {
            throw new Exception("Order already claimed");
        }

        var merchantId = order.OrderDetails
            .Select(x => x.Food.MerchantId)
            .FirstOrDefault();

        if (merchantId == Guid.Empty)
            throw new KeyNotFoundException("Merchant not found");
        var alreadyCheckedIn = await _dbContext.CheckIns
            .AnyAsync(x =>
                x.CustomerId == customerIdGuid &&
                x.MerchantId == merchantId &&
                x.CreatedAt >= DateTimeOffset.UtcNow.AddHours(-3));

        if (alreadyCheckedIn)
        {
            throw new Exception("Already checked in");
        }
        
        order.CustomerId = customerIdGuid;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        await CreateCheckIn(customerIdGuid, merchantId);
    }

    public async Task<List<Response.CurrentCheckInResponse>> GetCurrentCheckIns()
    {
        var merchantUserId = _httpContext.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "UserId")?.Value;

        if (string.IsNullOrWhiteSpace(merchantUserId))
            throw new UnauthorizedAccessException("UserId not found");

        var merchantUserIdGuid = Guid.Parse(merchantUserId);

        var merchant = await _dbContext.Merchants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == merchantUserIdGuid);

        if (merchant == null)
            throw new KeyNotFoundException("Merchant not found");

        return await _dbContext.CheckIns
            .AsNoTracking()
            .Include(x => x.Customer)
            .ThenInclude(x => x.User)
            .Where(x => x.MerchantId == merchant.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .Select(x => new Response.CurrentCheckInResponse
            {
                CheckInId = x.Id,
                MerchantId = x.MerchantId,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.User.FullName,
                CustomerEmail = x.Customer.User.Email,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync();
    }
}