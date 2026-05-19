using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UGem.Repositories;
using UGem.Repositories.Entity;
using UGem.Services.MonetizationService;

namespace UGem.Services.OrderService;

public class Service : IService
{
    private static readonly Regex OrderReferenceRegex =
        new(@"UGem-?(?<orderId>[A-Fa-f0-9]{32}|[A-Fa-f0-9\-]{36})", RegexOptions.Compiled);

    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly CheckInService.IService _checkInService;
    private readonly MonetizationService.IService _monetizationService;
    private readonly ILogger<Service> _logger;

    public Service(
        AppDbContext dbContext,
        IHttpContextAccessor httpContext,
        CheckInService.IService checkInService,
        MonetizationService.IService monetizationService,
        ILogger<Service> logger)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _checkInService = checkInService;
        _monetizationService = monetizationService;
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
                PaymentStatus = x.PaymentStatus,
                OrderType = x.OrderType,
                Status = x.Status,
                FinalPrice = x.FinalPrice,
                CustomerName = x.Customer != null && x.Customer.User != null
                    ? x.Customer.User.FullName
                    : x.Name,
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
        return await CreateOrderInternal(customerId, request, null);
    }

    public async Task<Response.CreateOrderResponse> CreateMerchantOrder(Request.CreateMerchantOrderRequest request)
    {
        var merchantUserId = GetRequiredGuidClaim("UserId");

        if (request.CustomerId == Guid.Empty)
        {
            throw new InvalidOperationException("CustomerId is required");
        }

        var merchant = await _dbContext.Merchants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == merchantUserId);

        if (merchant == null)
        {
            throw new KeyNotFoundException("Merchant not found");
        }

        var customerExists = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.CustomerId && x.User.IsActive);

        if (!customerExists)
        {
            throw new KeyNotFoundException("Customer not found");
        }

        return await CreateOrderInternal(request.CustomerId, request, merchant.Id);
    }

    private async Task<Response.CreateOrderResponse> CreateOrderInternal(
        Guid? customerId,
        Request.CreateOrderRequest request,
        Guid? merchantId)
    {
        if (request.Foods == null || request.Foods.Count == 0)
        {
            throw new InvalidOperationException("At least one food item is required");
        }

        var requestedFoodIds = request.Foods
            .Select(x => x.FoodId)
            .Distinct()
            .ToList();

        if (request.Foods.Any(x => x.Quantity <= 0))
        {
            throw new InvalidOperationException("Food quantity must be greater than 0");
        }

        var foods = await _dbContext.Foods
            .AsNoTracking()
            .Where(x => requestedFoodIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Price,
                x.MerchantId
            })
            .ToListAsync();

        if (foods.Count != requestedFoodIds.Count)
        {
            throw new KeyNotFoundException("Some food not found");
        }

        decimal totalAmount = 0;

        var merchantIds = foods.Select(x => x.MerchantId).Distinct().ToList();
        if (merchantIds.Count > 1)
        {
            throw new InvalidOperationException("An order can only contain items from a single merchant.");
        }

        var orderMerchantId = merchantIds.First();

        if (merchantId.HasValue && orderMerchantId != merchantId.Value)
        {
            throw new InvalidOperationException("All foods must belong to the merchant");
        }

        Guid? resolvedAffiliateLinkId = null;
        if (!string.IsNullOrWhiteSpace(request.AffiliateLinkCode))
        {
            var affiliateLink = await _dbContext.AffiliateLinks
                .FirstOrDefaultAsync(x => x.LinkCode == request.AffiliateLinkCode);

            if (affiliateLink == null)
            {
                throw new KeyNotFoundException("Affiliate link not found");
            }

            if (!affiliateLink.IsActive)
            {
                throw new InvalidOperationException("Affiliate link is not active");
            }

            if (affiliateLink.MerchantId != orderMerchantId)
            {
                throw new InvalidOperationException("Affiliate link does not belong to this merchant");
            }

            resolvedAffiliateLinkId = affiliateLink.Id;
        }

        var validOrderTypes = new[]
        {
            Request.OrderType.Online.ToString(),
            Request.OrderType.Offline.ToString()
        };

        if (!validOrderTypes.Contains(request.OrderType))
        {
            throw new InvalidOperationException("Invalid order type");
        }
var validPaymentMethods = new[]
        {
            "Cash",
            "BankTransfer",
            "COD"
        };

        if (!validPaymentMethods.Contains(request.PaymentMethod))
        {
            throw new InvalidOperationException("Invalid payment method");
        }

        if (request.OrderType == Request.OrderType.Online.ToString()
            && string.IsNullOrWhiteSpace(request.DeliveryAddress))
        {
            throw new InvalidOperationException("Online order must have delivery address");
        }

        if (request.OrderType == Request.OrderType.Offline.ToString()
            && string.Equals(request.PaymentMethod, "COD", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Offline order cannot use COD");
        }

        var orderId = Guid.NewGuid();
        var description = $"UGem-{orderId:N}";

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            DeliveryAddress = request.DeliveryAddress,
            OrderType = request.OrderType,
            Name = request.Name,
            Notes = request.Notes,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = request.PaymentMethod == "BankTransfer" ? "Pending" : "Unpaid",
            Status = Request.OrderStatus.Pending.ToString(),
            Discount = 0m,
            FinalPrice = totalAmount,
            ReviewerFee = 0m,
            OrderedAt = DateTimeOffset.UtcNow,
            PlatformFee = 0m,
            OrderDetails = new List<OrderDetail>(),
            AffiliateLinkId = resolvedAffiliateLinkId
        };
        foreach (var item in request.Foods)
        {
            var food = foods.First(x => x.Id == item.FoodId);

            var toppingIds = item.FoodToppingIds?
                .Distinct()
                .ToList() ?? new List<Guid>();

            var toppings = await _dbContext.FoodToppings
                .Where(x =>
                    toppingIds.Contains(x.Id) &&
                    x.FoodId == item.FoodId &&
                    x.IsActive &&
                    !x.IsDeleted)
                .ToListAsync();

            if (toppings.Count != toppingIds.Count)
            {
                throw new InvalidOperationException("Some toppings are invalid");
            }

            var toppingPrice = toppings.Sum(x => x.Price);

            var unitPrice = food.Price + toppingPrice;

            var subTotal = unitPrice * item.Quantity;

            var orderDetail = new OrderDetail
            {
                Id = Guid.NewGuid(),
                Name = food.Name,
                OrderId = orderId,
                FoodId = food.Id,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                Notes = item.Notes,
                OrderDetailToppings = new List<OrderDetailTopping>()
            };

            foreach (var topping in toppings)
            {
                orderDetail.OrderDetailToppings.Add(new OrderDetailTopping
{
                    Id = Guid.NewGuid(),
                    OrderDetailId = orderDetail.Id,
                    FoodToppingId = topping.Id,
                    Name = topping.Name,
                    Price = topping.Price,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false
                });
            }

            totalAmount += subTotal;

            order.OrderDetails.Add(orderDetail);
        }

        if (totalAmount <= 0)
        {
            throw new InvalidOperationException("Total amount must be greater than 0");
        }
            
        order.FinalPrice = totalAmount;
        
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var isBankTransfer = string.Equals(request.PaymentMethod, "BankTransfer", StringComparison.OrdinalIgnoreCase);

        return new Response.CreateOrderResponse
        {
            OrderId = order.Id,
            TotalAmount = order.FinalPrice,
            BankName = isBankTransfer ? "MBBank" : string.Empty,
            BankAccount = isBankTransfer ? "VQRQAIDAX4356" : string.Empty,
            Description = description,
            Code = order.Id.ToString("N"),
            QRCode = isBankTransfer
                ? $"https://qr.sepay.vn/img?acc=VQRQAIDAX4356&bank=MBBank&amount={(int)totalAmount}&des={description}&template=qronly"
                : null
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

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            order.PaymentStatus = "Paid";
            order.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            await _monetizationService.HandlePaymentSuccess(order.Id);
            
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing SePay payment success for order {OrderId}", order.Id);
            throw;
        }
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

                Toppings = x.OrderDetailToppings.Select(t =>
                    new Response.OrderDetailToppingResponse
                    {
                        Name = t.Name,
                        Price = t.Price
                    }).ToList()
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

        if (order.OrderType != Request.OrderType.Online.ToString())
        {
            throw new InvalidOperationException("This API is only for online orders");
        }

        if (order.Status == Request.OrderStatus.NotReceived.ToString()
            || order.Status == Request.OrderStatus.Rejected.ToString()
            || order.Status == Request.OrderStatus.Completed.ToString())
        {
            throw new InvalidOperationException("Order cannot be confirmed in its current state");
        }

        order.Status = Request.OrderStatus.Completed.ToString();

        if (string.Equals(order.PaymentMethod, "COD", StringComparison.OrdinalIgnoreCase))
        {
            order.PaymentStatus = "Paid";
        }

        order.UpdatedAt = DateTimeOffset.UtcNow;

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
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

            await _monetizationService.HandlePaymentSuccess(order.Id);
            
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing order received confirmation for order {OrderId}", order.Id);
            throw;
        }
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

        
        if (order.OrderType != Request.OrderType.Online.ToString())
        {
            throw new InvalidOperationException("This API is only for online orders");
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
            .ThenInclude(x => x.OrderDetailToppings)
            .FirstOrDefaultAsync(x =>
                x.Id == request.OrderId &&
                x.CustomerId == customerId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        if (order.OrderType != Request.OrderType.Offline.ToString())
        {
            throw new InvalidOperationException("Bill API is only for offline orders");
        }

        var selectOrder = new Response.GetOrderBillResponse
        {
            OrderId = order.Id,
            Name = order.Name,
            PaymentMethod = order.PaymentMethod,
            OrderedAt = order.OrderedAt,
            DeliveryAddress = order.DeliveryAddress,
            Discount = order.Discount,
            FinalPrice = order.FinalPrice,
            Items = order.OrderDetails.Select(x => new Response.GetBillItemResponse
            {
                Name = x.Name,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                SubTotal = x.UnitPrice * x.Quantity,
                Notes = x.Notes,
                Toppings = x.OrderDetailToppings.Select(t => new Response.OrderDetailToppingResponse
                {
                    Name = t.Name,
                    Price = t.Price
                }).ToList()
            }).ToList(),
            Notes = order.Notes
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

        if (order.OrderType != Request.OrderType.Offline.ToString())
        {
throw new InvalidOperationException("Bill API is only for offline orders");
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
    public async Task<Response.GetOrderBillResponse> GetMerchantOrderDetail(Guid orderId)
    {
        var merchantUserId = GetRequiredGuidClaim("UserId");

        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.OrderDetailToppings)
            .FirstOrDefaultAsync(x =>
                x.Id == orderId &&
                x.OrderDetails.Any(od => od.Food.Merchant.UserId == merchantUserId));

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        return new Response.GetOrderBillResponse
        {
            OrderId = order.Id,
            Name = order.Name,
            Notes = order.Notes,
            PaymentMethod = order.PaymentMethod,
            OrderedAt = order.OrderedAt,
            DeliveryAddress = order.DeliveryAddress,
            Discount = order.Discount,
            FinalPrice = order.FinalPrice,
            Items = order.OrderDetails.Select(x => new Response.GetBillItemResponse()
            {
                Name = x.Name,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                SubTotal = x.UnitPrice * x.Quantity,
                Notes = x.Notes,
                Toppings = x.OrderDetailToppings.Select(t => new Response.OrderDetailToppingResponse()
                {
                    Name = t.Name,
                    Price = t.Price
                }).ToList()
            }).ToList()
        };
    }

    public async Task RequestCashPayment(Request.ConfirmOrderRequest request)
    {
        var customerId = GetRequiredGuidClaim("CustomerId");
        var userId = GetRequiredGuidClaim("UserId");

        var order = await _dbContext.Orders
            .Include(x => x.Customer)
.Include(x => x.OrderDetails)
            .ThenInclude(x => x.Food)
            .ThenInclude(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.CustomerId == customerId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        if (order.OrderType != Request.OrderType.Offline.ToString())
        {
            throw new InvalidOperationException("Cash payment request is only for offline orders");
        }

        if (!string.Equals(order.PaymentMethod, "Cash", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cash payment request is only available for cash orders");
        }

        if (order.Status != Request.OrderStatus.BillConfirmed.ToString())
        {
            throw new InvalidOperationException("Cash payment can only be requested after bill is confirmed");
        }

        order.Status = Request.OrderStatus.CashPending.ToString();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        var merchant = order.OrderDetails.Select(x => x.Food.Merchant).FirstOrDefault();

        var notification = new Notification
        {
            UserId = merchant!.UserId,
            Title = "Cash payment requested",
            Message = $"{user!.FullName} marked order #{order.Id} as paid in cash. Please confirm the payment.",
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

        if (order.OrderType != Request.OrderType.Offline.ToString())
        {
            throw new InvalidOperationException("Bill API is only for offline orders");
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
            .ThenInclude(x => x.OrderDetailToppings)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId);
 
        if (order == null)
        {
            throw new KeyNotFoundException("Order not found ");
        }

        if (order.OrderType != Request.OrderType.Offline.ToString())
        {
            throw new InvalidOperationException("Bill API is only for offline orders");
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
            Items = order.OrderDetails.Select(x => new Response.GetBillItemResponse
            {
                Name = x.Name,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                SubTotal = x.UnitPrice * x.Quantity,
                Notes = x.Notes,
                Toppings = x.OrderDetailToppings.Select(t => new Response.OrderDetailToppingResponse
                {
                    Name = t.Name,
                    Price = t.Price
                }).ToList()
            }).ToList()
        };
    }

    public async Task ConfirmCashPayment(Guid orderId)
    {
        var merchantUserId = GetRequiredGuidClaim("UserId");

        var order = await _dbContext.Orders.Include(order => order.Customer).Include(order => order.OrderDetails)
            .ThenInclude(orderDetail => orderDetail.Food)
            .FirstOrDefaultAsync(x =>
                x.Id == orderId &&
                x.OrderDetails.Any(od => od.Food.Merchant.UserId == merchantUserId));

        if (order == null)
            throw new KeyNotFoundException("Order not found or not yours");

        if (order.OrderType != Request.OrderType.Offline.ToString())
            throw new InvalidOperationException("Cash payment confirmation is only for offline orders");

        if (!string.Equals(order.PaymentMethod, "Cash", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cash payment confirmation is only available for cash orders");

        if (order.Status != Request.OrderStatus.CashPending.ToString())
            throw new InvalidOperationException("Cash payment can only be confirmed after the customer marks it as paid");

        if (!order.CustomerId.HasValue || !order.CustomerId.HasValue)
            throw new InvalidOperationException("Order has not been claimed by a customer");

        var merchantId = order.OrderDetails
            .Select(od => od.Food.MerchantId)
            .FirstOrDefault();

        if (merchantId == Guid.Empty)
            throw new KeyNotFoundException("Merchant not found");

        order.Status = Request.OrderStatus.Completed.ToString();
        order.PaymentStatus = "Paid";
        order.UpdatedAt = DateTimeOffset.UtcNow;

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            await _checkInService.CreateCheckIn(order.CustomerId.Value, merchantId);

            _dbContext.Notifications.Add(new Notification
            {
                UserId = order.Customer.UserId,
                Title = "Order completed",
                Message = $"Your cash payment for order #{order.Id} has been confirmed.",
                Type = "order",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow,
            });
            await _dbContext.SaveChangesAsync();
            await _monetizationService.HandlePaymentSuccess(order.Id);
            
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error confirming cash payment for order {OrderId}", order.Id);
            throw;
        }
    }

    public async Task RefundOrder(Guid orderId)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) throw new KeyNotFoundException("Order not found");

        if (order.PaymentStatus != "Paid")
        {
            throw new InvalidOperationException("Only paid orders can be refunded");
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            order.Status = "Refunded";
            order.PaymentStatus = "Refunded";
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();
            await _monetizationService.HandleRefund(order.Id);
            
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error refunding order {OrderId}", order.Id);
            throw;
        }
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