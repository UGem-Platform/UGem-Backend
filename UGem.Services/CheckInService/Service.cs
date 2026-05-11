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

    public byte[] GenerateQrCode(Request.GenerateQrCodeRequest request)
    {
        var qrText = $"https://u-gem.vercel.app/check-in?merchantId={request.MerchantId}";
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

    public async Task FireCheckIn()
    {
        var cusId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;
        
        var cusIdGuid = Guid.Parse(cusId!);
        
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == cusIdGuid);
        
        if (customer != null)
        {
            customer.TotalCheckIns += 1;
        } 
        await _dbContext.SaveChangesAsync();    
    }
}