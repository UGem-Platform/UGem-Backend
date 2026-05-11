using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<Service> _logger;

    public Service(
        AppDbContext dbContext,
        IHttpContextAccessor httpContext,
        CheckInService.IService checkInService,
        ILogger<Service> logger)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _checkInService = checkInService;
        _logger = logger;
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
        if (request.TransferAmount <= 0)
        {
            _logger.LogWarning(
                "Rejected SePay webhook with non-positive amount. ReferenceCode={ReferenceCode}, TransferAmount={TransferAmount}",
                request.ReferenceCode,
                request.TransferAmount);
            throw new InvalidOperationException("Transfer amount must be greater than 0");
        }

        var orderId = ExtractOrderId(request.Content);
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning(
                "Rejected SePay webhook because order was not found. ParsedOrderId={OrderId}, ReferenceCode={ReferenceCode}",
                orderId,
                request.ReferenceCode);
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

            _logger.LogWarning(
                "Rejected SePay webhook due to amount mismatch. OrderId={OrderId}, ExpectedAmount={ExpectedAmount}, ActualAmount={ActualAmount}",
                order.Id,
                order.FinalPrice,
                request.TransferAmount);
            throw new InvalidOperationException("Invalid transfer amount");
        }

        if (order.Status == Request.OrderStatus.Completed.ToString())
        {
            _logger.LogInformation(
                "Ignored duplicate SePay webhook for completed order. OrderId={OrderId}",
                order.Id);
            return;
        }

        if (order.Status != Request.OrderStatus.Pending.ToString())
        {
            _logger.LogWarning(
                "Rejected SePay webhook because order is already in state {OrderStatus}. OrderId={OrderId}",
                order.Status,
                order.Id);
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

    public async Task<Response.GetOrderBillResponse> GetBill(Request.GetBillByOrderIdRequest request)
    {
        var customerId = GetRequiredGuidClaim("CustomerId");
        
        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId &&  x.CustomerId == customerId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        var selectOrder = new Response.GetOrderBillResponse()
        {
            OrderId = order.Id,
            Name = order.Name,
            PaymentMethod = order.PaymentMethod,
            OrderedAt = order.OrderedAt,
            DeliveryAddress = order.DeliveryAddress,
            Discount = order.Discount,
            FinalPrice = order.FinalPrice,
            Items = order.OrderDetails.Select(x =>
                    new Response.BillItemResponse()
                    {
                        Name = x.Name,
                        Quantity = x.Quantity,
                        SubTotal = x.UnitPrice * x.Quantity,
                        UnitPrice = x.UnitPrice
                    })
                .ToList()
        };

        return selectOrder;
    }
    
    
    public async Task ConfirmBill(Request.ConfirmBillRequest request)
    {
        var customerId = GetRequiredGuidClaim("CustomerId");
        var userId = GetRequiredGuidClaim("UserId");
 
        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.Food)
            .ThenInclude(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.CustomerId == customerId);
 
        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }
 
        if (order.Status != Request.OrderStatus.Accepted.ToString()
            && order.Status != Request.OrderStatus.BillUpdated.ToString()
            && order.Status != Request.OrderStatus.BillRejected.ToString())
        {
            throw new InvalidOperationException("Bill can only be confirmed when order is Accepted or BillUpdated");
        }
 
        order.Status = Request.OrderStatus.BillConfirmed.ToString();
        order.UpdatedAt = DateTimeOffset.UtcNow;
 
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        var merchant = order.OrderDetails.Select(x => x.Food.Merchant).FirstOrDefault();
 
        var notification = new Notification
        {
            UserId = merchant!.UserId,
            Title = "Bill confirmed",
            Message = $"{user!.FullName} has confirmed the updated bill for order #{order.Id}",
            Type = "order",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
    }
 
    public async Task RejectBill(Request.RejectBillRequest request)
    {
        var customerId = GetRequiredGuidClaim("CustomerId");
        var userId = GetRequiredGuidClaim("UserId");
 
        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.Food)
            .ThenInclude(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.CustomerId == customerId);
 
        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }
 
        if (order.Status != Request.OrderStatus.Accepted.ToString()
            && order.Status != Request.OrderStatus.BillUpdated.ToString()
            && order.Status != Request.OrderStatus.BillRejected.ToString())
        {
            throw new InvalidOperationException("Bill can only be rejected when order is Accepted or BillUpdated");
        }
 
        order.Status = Request.OrderStatus.BillRejected.ToString();
        order.RejectionReason = request.Reason;
        order.UpdatedAt = DateTimeOffset.UtcNow;
 
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        var merchant = order.OrderDetails.Select(x => x.Food.Merchant).FirstOrDefault();
 
        var notification = new Notification
        {
            UserId = merchant!.UserId,
            Title = "Bill rejected",
            Message = $"{user!.FullName} has rejected the updated bill for order #{order.Id} with reason:  {request.Reason}",
            Type = "order",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
    }
 
    public async Task<Response.UpdateBillResponse> UpdateBill(Request.UpdateBillRequest request)
    {
        var merchantUserId = GetRequiredGuidClaim("UserId");
 
        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.Food)
            .ThenInclude(x => x.Merchant)
            .FirstOrDefaultAsync(x =>
                x.Id == request.OrderId);
 
        if (order == null)
        {
            throw new KeyNotFoundException("Order not found ");
        }
 
        if (order.Status != Request.OrderStatus.BillRejected.ToString())
        {
            throw new InvalidOperationException(
                "Bill can only be updated when status is BillRejected");
        }

        if (request.Discount.HasValue)
        {
            if (request.Discount.Value < 0)
                throw new InvalidOperationException("Discount cannot be negative");
 
            order.Discount = request.Discount.Value;
        }
 
        if (request.Items != null && request.Items.Count > 0)
        {
            foreach (var item in request.Items)
            {
                var orderDetail = order.OrderDetails
                    .FirstOrDefault(x => x.FoodId == item.FoodId);

                if (orderDetail == null)
                    throw new KeyNotFoundException("Order item not found");

                if (item.Quantity.HasValue)
                    orderDetail.Quantity = item.Quantity.Value;

                if (item.UnitPrice.HasValue)
                    orderDetail.UnitPrice = item.UnitPrice.Value;
            }
        }

        order.FinalPrice = order.OrderDetails.Sum(x => x.UnitPrice * x.Quantity) - order.Discount;
        order.Status = Request.OrderStatus.BillUpdated.ToString();
        order.UpdatedAt = DateTimeOffset.UtcNow;
 
        var customer = await _dbContext.Customers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == order.CustomerId);
 
        var notification = new Notification
        {
            UserId = customer!.UserId,
            Title = "Bill updated",
            Message = $"Your bill for order #{order.Id} has been updated.",
            Type = "order",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
 
        return new Response.UpdateBillResponse
        {
            OrderId = order.Id,
            Discount = order.Discount,
            FinalPrice = order.FinalPrice,
            Items = order.OrderDetails.Select(x => new Response.BillItemResponse
            {
                Name = x.Name,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                SubTotal = x.UnitPrice * x.Quantity,
            }).ToList()
        };
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
