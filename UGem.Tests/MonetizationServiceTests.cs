using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;
using UGem.Services.MonetizationService;
using Xunit;

namespace UGem.Tests;

public class MonetizationServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public MonetizationServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbOptions);

    [Theory]
    [InlineData(10, 0.0)] // Bronze
    [InlineData(30, 0.005)] // Silver
    [InlineData(75, 0.01)] // Gold
    [InlineData(150, 0.02)] // Diamond
    public async Task HandlePaymentSuccess_CalculatesCorrectReviewerFeeBasedOnRank(int points, decimal expectedRate)
    {
        // Arrange
        using var context = CreateContext();
        var merchant = CreateMerchant(context, 5.0m);
        var reviewer = CreateReviewer(context, points);
        var affiliateLink = CreateAffiliateLink(context, reviewer.Id, merchant.Id);
        var order = CreateOrder(context, merchant.Id, affiliateLink.Id, 100000m);
        await context.SaveChangesAsync();

        var service = new Service(context);

        // Act
        await service.HandlePaymentSuccess(order.Id);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        decimal expectedFee = 100000m * expectedRate;
        Assert.Equal(expectedFee, updatedOrder!.ReviewerFee);
        Assert.Equal(5000m, updatedOrder.PlatformFee); // 5% of 100,000
        Assert.NotNull(updatedOrder.MonetizationProcessedAtUtc);

        var transaction = await context.ReviewerWalletTransactions
            .FirstOrDefaultAsync(t => t.OrderId == order.Id && t.Type == ReviewerWalletTransactionType.Commission);
        Assert.NotNull(transaction);
        Assert.Equal(expectedFee, transaction.Amount);
    }

    [Fact]
    public async Task HandlePaymentSuccess_SkipsCommissionForSelfReferral()
    {
        // Arrange
        using var context = CreateContext();
        var merchant = CreateMerchant(context, 5.0m);
        var reviewer = CreateReviewer(context, 100); // Diamond
        var affiliateLink = CreateAffiliateLink(context, reviewer.Id, merchant.Id);
        
        // Buyer is the same as Reviewer owner
        var order = CreateOrder(context, merchant.Id, affiliateLink.Id, 100000m, reviewer.CustomerId);
        await context.SaveChangesAsync();

        var service = new Service(context);

        // Act
        await service.HandlePaymentSuccess(order.Id);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.Equal(0m, updatedOrder!.ReviewerFee);
        
        var transaction = await context.ReviewerWalletTransactions
            .FirstOrDefaultAsync(t => t.OrderId == order.Id && t.Type == ReviewerWalletTransactionType.Commission);
        Assert.NotNull(transaction);
        Assert.Equal(0m, transaction.Amount);
        Assert.Contains("Self-referral", transaction.Reason!);
    }

    [Fact]
    public async Task HandlePaymentSuccess_IsIdempotent()
    {
        // Arrange
        using var context = CreateContext();
        var merchant = CreateMerchant(context, 5.0m);
        var reviewer = CreateReviewer(context, 100);
        var affiliateLink = CreateAffiliateLink(context, reviewer.Id, merchant.Id);
        var order = CreateOrder(context, merchant.Id, affiliateLink.Id, 100000m);
        await context.SaveChangesAsync();

        var service = new Service(context);

        // Act
        await service.HandlePaymentSuccess(order.Id);
        var firstProcessedAt = (await context.Orders.FindAsync(order.Id))!.MonetizationProcessedAtUtc;
        
        await service.HandlePaymentSuccess(order.Id); // Call again

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.Equal(firstProcessedAt, updatedOrder!.MonetizationProcessedAtUtc);
        
        var transactions = await context.ReviewerWalletTransactions
            .Where(t => t.OrderId == order.Id && t.Type == ReviewerWalletTransactionType.Commission)
            .ToListAsync();
        Assert.Single(transactions);
    }

    [Fact]
    public async Task HandleRefund_ReversesCommissionCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var merchant = CreateMerchant(context, 5.0m);
        var reviewer = CreateReviewer(context, 100);
        var affiliateLink = CreateAffiliateLink(context, reviewer.Id, merchant.Id);
        var order = CreateOrder(context, merchant.Id, affiliateLink.Id, 100000m);
        await context.SaveChangesAsync();

        var service = new Service(context);
        await service.HandlePaymentSuccess(order.Id);
        
        var balanceAfterCommission = (await context.Reviewers.FindAsync(reviewer.Id))!.Balance;
        Assert.Equal(2000m, balanceAfterCommission); // 2% of 100,000

        // Act
        await service.HandleRefund(order.Id);

        // Assert
        var updatedReviewer = await context.Reviewers.FindAsync(reviewer.Id);
        Assert.Equal(0m, updatedReviewer!.Balance);

        var reversal = await context.ReviewerWalletTransactions
            .FirstOrDefaultAsync(t => t.OrderId == order.Id && t.Type == ReviewerWalletTransactionType.Reversal);
        Assert.NotNull(reversal);
        Assert.Equal(2000m, reversal.Amount);
        Assert.Equal(0m, reversal.BalanceAfter);
    }

    [Fact]
    public async Task HandlePaymentSuccess_ThrowsIfMultipleMerchants()
    {
        // Arrange
        using var context = CreateContext();
        var merchant1 = CreateMerchant(context, 5.0m);
        var merchant2 = CreateMerchant(context, 5.0m);
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Name = "Multi-Merchant Order",
            FinalPrice = 100000m,
            Status = "Paid",
            PaymentMethod = "Cash",
            OrderedAt = DateTimeOffset.UtcNow,
            Notes = "",
            DeliveryAddress = "Test"
        };
        order.OrderDetails.Add(new OrderDetail { Id = Guid.NewGuid(), OrderId = order.Id, FoodId = Guid.NewGuid(), Name = "Food 1", Food = new Food { Id = Guid.NewGuid(), Name = "F1", MerchantId = merchant1.Id, Description = "D", Merchant = merchant1 } });
        order.OrderDetails.Add(new OrderDetail { Id = Guid.NewGuid(), OrderId = order.Id, FoodId = Guid.NewGuid(), Name = "Food 2", Food = new Food { Id = Guid.NewGuid(), Name = "F2", MerchantId = merchant2.Id, Description = "D", Merchant = merchant2 } });
        
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new Service(context);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.HandlePaymentSuccess(order.Id));
    }

    // Helpers
    private Merchant CreateMerchant(AppDbContext context, decimal feePercent)
    {
        var m = new Merchant
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Test Merchant",
            Description = "Test Description",
            PlatformFeePercent = feePercent,
            Email = $"{Guid.NewGuid()}@test.com",
            Phone = "123",
            Address = "Test",
            LogoUrl = "test.png",
            Status = "Active",
            OpeningHours = "9-5",
            Location = new NetTopologySuite.Geometries.Point(0, 0) { SRID = 4326 }
        };
        context.Merchants.Add(m);
        return m;
    }

    private Reviewer CreateReviewer(AppDbContext context, int points)
    {
        var r = new Reviewer
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Customer = new Customer { Id = Guid.NewGuid(), UserId = Guid.NewGuid() },
            Points = points,
            Rank = points >= 100 ? "Diamond" : "Bronze",
            Balance = 0
        };
        context.Reviewers.Add(r);
        return r;
    }

    private AffiliateLink CreateAffiliateLink(AppDbContext context, Guid reviewerId, Guid merchantId)
    {
        var al = new AffiliateLink
        {
            Id = Guid.NewGuid(),
            LinkCode = Guid.NewGuid().ToString("N")[..8],
            ReviewerId = reviewerId,
            MerchantId = merchantId,
            IsActive = true
        };
        context.AffiliateLinks.Add(al);
        return al;
    }

    private Order CreateOrder(AppDbContext context, Guid merchantId, Guid? affiliateLinkId, decimal price, Guid? customerId = null)
    {
        var oId = Guid.NewGuid();
        var order = new Order
        {
            Id = oId,
            Name = "Test Order",
            FinalPrice = price,
            Status = "Paid",
            PaymentMethod = "Cash",
            OrderedAt = DateTimeOffset.UtcNow,
            AffiliateLinkId = affiliateLinkId,
            CustomerId = customerId ?? Guid.NewGuid(),
            Notes = "",
            DeliveryAddress = "Test"
        };
        
        // Add one item for the merchant
        var foodId = Guid.NewGuid();
        order.OrderDetails.Add(new OrderDetail
        {
            Id = Guid.NewGuid(),
            OrderId = oId,
            FoodId = foodId,
            Name = "Item 1",
            Quantity = 1,
            UnitPrice = price,
            Food = new Food { Id = foodId, Name = "F1", MerchantId = merchantId, Description = "D" }
        });

        context.Orders.Add(order);
        return order;
    }
}
