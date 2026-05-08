using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.OrderService;

public class Service : IService
{
    private static readonly Regex OrderReferenceRegex =
        new(@"UGem-?(?<orderId>[A-Fa-f0-9]{32}|[A-Fa-f0-9\-]{36})", RegexOptions.Compiled);

    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly CheckInService.IService _checkInService;
    private readonly SepayWebhookOptions _webhookOptions;

    public Service(
        AppDbContext dbContext,
        IHttpContextAccessor httpContext,
        CheckInService.IService checkInService,
        IOptions<SepayWebhookOptions> webhookOptions)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _checkInService = checkInService;
        _webhookOptions = webhookOptions.Value;
    }

    public async Task<List<Response.GetOrderListResponse>> GetOrdersList()
    {
        var userIdGuid = GetRequiredGuidClaim("UserId");

        return await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.OrderDetails.Any(od => od.Food.Merchant.UserId == userIdGuid))
            .OrderByDescending(o => o.CreatedAt)
            .Select(x => new Response.GetOrderListResponse
            {
                OrderId = x.Id,
                DeliveryAddress = x.DeliveryAddress,
                PaymentMethod = x.PaymentMethod,
                Status = x.Status,
                FinalPrice = x.FinalPrice,
                CustomerName = x.Customer.User.FullName,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync();
    }

    public async Task AcceptOrder(Guid orderId)
    {
        var userIdGuid = GetRequiredGuidClaim("UserId");

        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId
                                      && x.OrderDetails.Any(od => od.Food.Merchant.UserId == userIdGuid));

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found or not yours");
        }

        if (order.Status != Request.OrderStatus.Pending.ToString())
        {
            throw new InvalidOperationException("Order is not in Pending state");
        }

        order.Status = Request.OrderStatus.Accepted.ToString();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task RejectOrder(Request.ReasonRejectRequest request)
    {
        var userIdGuid = GetRequiredGuidClaim("UserId");

        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == request.OrderId
                                      && x.OrderDetails.Any(od => od.Food.Merchant.UserId == userIdGuid));

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found or not yours");
        }

        if (order.Status != Request.OrderStatus.Pending.ToString())
        {
            throw new InvalidOperationException("Order is not in Pending state");
        }

        order.Status = Request.OrderStatus.Rejected.ToString();
        order.RejectionReason = request.Reason;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<Response.CreateOrderResponse> CreateOrder(Request.CreateOrderRequest request)
    {
        var customerId = GetRequiredGuidClaim("CustomerId");

        if (request.Foods == null || request.Foods.Count == 0)
        {
            throw new InvalidOperationException("At least one food item is required");
        }

        var requestedItems = request.Foods
            .GroupBy(x => x.FoodId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        if (requestedItems.Values.Any(quantity => quantity <= 0))
        {
            throw new InvalidOperationException("Food quantity must be greater than 0");
        }

        var foods = await _dbContext.Foods
            .AsNoTracking()
            .Where(x => requestedItems.Keys.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Price
            })
            .ToListAsync();

        if (foods.Count != requestedItems.Count)
        {
            throw new KeyNotFoundException("Some food not found");
        }

        var totalAmount = foods.Sum(food => requestedItems[food.Id] * food.Price);
        if (totalAmount <= 0)
        {
            throw new InvalidOperationException("Total amount must be greater than 0");
        }

        var orderId = Guid.NewGuid();
        var description = $"UGem-{orderId:N}";

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            DeliveryAddress = request.DeliveryAddress,
            Name = request.Name,
            Notes = request.Notes,
            PaymentMethod = request.PaymentMethod,
            Status = Request.OrderStatus.Pending.ToString(),
            Discount = 0m,
            FinalPrice = totalAmount,
            ReviewerFee = 0m,
            OrderedAt = DateTimeOffset.UtcNow,
            PlatformFee = 0m,
            OrderDetails = foods.Select(food => new OrderDetail
            {
                Id = Guid.NewGuid(),
                Name = food.Name,
                OrderId = orderId,
                FoodId = food.Id,
                Quantity = requestedItems[food.Id],
                UnitPrice = food.Price,
            }).ToList()
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        return new Response.CreateOrderResponse
        {
            OrderId = order.Id,
            TotalAmount = order.FinalPrice,
            BankName = "MBBank",
            BankAccount = "VQRQAIDAX4356",
            Description = description,
            Code = order.Id.ToString("N"),
            QRCode = $"https://qr.sepay.vn/img?acc=VQRQAIDAX4356&bank=MBBank&amount={(int)totalAmount}&des={description}&template=qronly"
        };
    }

    public async Task SepayWebhookHandler(Request.SepayWebhookRequest request)
    {
        ValidateWebhookSecret();

        if (request.TransferAmount <= 0)
        {
            throw new InvalidOperationException("Transfer amount must be greater than 0");
        }

        var orderId = ExtractOrderId(request.Content);
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        if (order.FinalPrice != request.TransferAmount)
        {
            if (order.Status != "Failed")
            {
                order.Status = "Failed";
                order.UpdatedAt = DateTimeOffset.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            throw new InvalidOperationException("Invalid transfer amount");
        }

        if (order.Status == Request.OrderStatus.Completed.ToString())
        {
            return;
        }

        if (order.Status != Request.OrderStatus.Pending.ToString())
        {
            throw new InvalidOperationException("Order already processed");
        }

        order.Status = Request.OrderStatus.Completed.ToString();
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Response.OrderResponse>> GetOrderListFromCustomerId()
    {
        var customerId = GetRequiredGuidClaim("CustomerId");

        return await _dbContext.Orders
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new Response.OrderResponse
            {
                OrderId = x.Id,
                Id = x.Id,
                Name = x.Name,
                DeliveryAddress = x.DeliveryAddress,
                Notes = x.Notes,
                Status = x.Status,
                Discount = x.Discount,
                FinalPrice = x.FinalPrice,
                OrderedAt = x.OrderedAt,
            })
            .ToListAsync();
    }

    public async Task<List<Response.GetOrderDetailResponse>> GetOrderDetail(Guid orderId)
    {
        var customerId = GetRequiredGuidClaim("CustomerId");

        return await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(x => x.OrderId == orderId && x.Order.CustomerId == customerId)
            .Select(x => new Response.GetOrderDetailResponse
            {
                Name = x.Name,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                Notes = x.Notes,
                OrderId = x.OrderId,
                FoodId = x.FoodId,
            })
            .ToListAsync();
    }

    public async Task ConfirmOrderReceived(Request.ConfirmOrderRequest request)
    {
        var customerId = GetRequiredGuidClaim("CustomerId");

        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.Food)
            .ThenInclude(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.CustomerId == customerId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        if (order.Status == Request.OrderStatus.NotReceived.ToString()
            || order.Status == Request.OrderStatus.Rejected.ToString())
        {
            throw new InvalidOperationException("Order cannot be confirmed in its current state");
        }

        order.Status = Request.OrderStatus.Completed.ToString();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        var merchantId = order.OrderDetails
            .Select(od => od.Food.MerchantId)
            .FirstOrDefault();

        if (merchantId != Guid.Empty)
        {
            await _checkInService.CreateCheckIn(customerId, merchantId);
        }

        var userId = GetRequiredGuidClaim("UserId");
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);

        var merchant = order.OrderDetails
            .Select(x => x.Food.Merchant)
            .FirstOrDefault();

        var notificationMerchant = new Notification
        {
            UserId = merchant!.UserId,
            Title = "Order completed",
            Message = $"{user!.FullName} has received the order",
            Type = "order",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Notifications.Add(notificationMerchant);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ConfirmOrderNotReceived(Request.ConfirmOrderRequest request)
    {
        var customerId = GetRequiredGuidClaim("CustomerId");

        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.Food)
            .ThenInclude(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.CustomerId == customerId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        if (order.Status == Request.OrderStatus.NotReceived.ToString()
            || order.Status == Request.OrderStatus.Rejected.ToString())
        {
            throw new InvalidOperationException("Order cannot be marked as not received in its current state");
        }

        order.Status = Request.OrderStatus.NotReceived.ToString();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        var userId = GetRequiredGuidClaim("UserId");
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);

        var merchant = order.OrderDetails
            .Select(x => x.Food.Merchant)
            .FirstOrDefault();

        var notificationMerchant = new Notification
        {
            UserId = merchant!.UserId,
            Title = "Order issue",
            Message = $"{user!.FullName} has not received the order",
            Type = "order",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Notifications.Add(notificationMerchant);
        await _dbContext.SaveChangesAsync();
    }

    private void ValidateWebhookSecret()
    {
        var request = _httpContext.HttpContext?.Request
                      ?? throw new UnauthorizedAccessException("Webhook request context is unavailable");

        if (!request.Headers.TryGetValue(_webhookOptions.HeaderName, out var providedSecret))
        {
            throw new UnauthorizedAccessException("Webhook signature is missing");
        }

        if (!FixedTimeEquals(providedSecret.ToString(), _webhookOptions.SharedSecret))
        {
            throw new UnauthorizedAccessException("Webhook signature is invalid");
        }
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        return leftBytes.Length == rightBytes.Length
               && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static Guid ExtractOrderId(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Order reference is missing from webhook content");
        }

        var normalizedContent = content
            .Replace(" ", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty);

        var match = OrderReferenceRegex.Match(normalizedContent);
        if (!match.Success)
        {
            throw new InvalidOperationException("UGem order reference was not found");
        }

        var rawOrderId = match.Groups["orderId"].Value;
        if (Guid.TryParse(rawOrderId, out var guid))
        {
            return guid;
        }

        if (rawOrderId.Length == 32)
        {
            var formatted = $"{rawOrderId[..8]}-{rawOrderId.Substring(8, 4)}-{rawOrderId.Substring(12, 4)}-{rawOrderId.Substring(16, 4)}-{rawOrderId.Substring(20, 12)}";
            if (Guid.TryParse(formatted, out guid))
            {
                return guid;
            }
        }

        throw new InvalidOperationException("Webhook order reference format is invalid");
    }

    private Guid GetRequiredGuidClaim(string claimType)
    {
        var rawValue = _httpContext.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw new UnauthorizedAccessException($"{claimType} claim is missing");
        }

        return Guid.Parse(rawValue);
    }
}
