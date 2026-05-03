using Microsoft.EntityFrameworkCore;
using QRCoder;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.CheckInService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
}