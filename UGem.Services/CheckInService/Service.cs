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

    public byte[] GenerateQrCode(string text)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
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
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.CustomerId == customerIdGuid);

        if (order == null)
            throw new KeyNotFoundException("Order not found");

        var merchantId = order.OrderDetails
            .Select(x => x.Food.MerchantId)
            .FirstOrDefault();

        if (merchantId == Guid.Empty)
            throw new KeyNotFoundException("Merchant not found");

        await CreateCheckIn(customerIdGuid, merchantId);
    }
}
