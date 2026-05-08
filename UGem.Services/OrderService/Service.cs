using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.OrderService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly CheckInService.IService _checkInService;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext, CheckInService.IService checkInService)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _checkInService = checkInService;
    }

    public async Task<List<Response.GetOrderListResponse>> GetOrdersList()
    {
        var userId = _httpContext.HttpContext.User.Claims.First(x => x.Type == "UserId").Value; 
        var userIdGuid = Guid.Parse(userId);
        var query = _dbContext.Orders
            .Where(o => o.OrderDetails.Any(od => od.Food.Merchant.UserId == userIdGuid));
        query = query.OrderByDescending(o => o.CreatedAt);
        
        var selectQuery = query.Select(x => new Response.GetOrderListResponse()
        {
            OrderId = x.Id,
            DeliveryAddress = x.DeliveryAddress,
            PaymentMethod = x.PaymentMethod,
            Status = x.Status,
            FinalPrice =  x.FinalPrice,
            CustomerName = x.Customer.User.FullName,
            CreatedAt = x.CreatedAt,
        });
        var listOrder = await selectQuery.ToListAsync();
        return listOrder;
    }

    public async Task AcceptOrder(Guid orderId)
    {
        var userId = _httpContext.HttpContext.User
            .Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("Unauthorized");
        }
        var userIdGuid = Guid.Parse(userId);

        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .ThenInclude(od => od.Food)
            .ThenInclude(f => f.Merchant)
            .FirstOrDefaultAsync(x => x.Id == orderId
                                      && x.OrderDetails.Any(od => od.Food.Merchant.UserId == userIdGuid));

        if (order == null)
        {
            throw new Exception("Order not found or not yours");
        }

        if (order.Status != Request.OrderStatus.Pending.ToString())
        {
            throw new Exception("Order is not in Pending state");
        }

        order.Status = Request.OrderStatus.Accepted.ToString();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task RejectOrder(Request.ReasonRejectRequest request)
    {
        var userId = _httpContext.HttpContext.User
            .Claims.First(x => x.Type == "UserId").Value;

        var userIdGuid = Guid.Parse(userId);

        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == request.OrderId
                                      && x.OrderDetails.Any(od => od.Food.Merchant.UserId == userIdGuid));

        if (order == null)
        {
            throw new Exception("Order not found or not yours");
        }

        if (order.Status != Request.OrderStatus.Pending.ToString())
        {
            throw new Exception("Order is not in Pending state");
        }

        order.Status = Request.OrderStatus.Rejected.ToString();
        order.RejectionReason = request.Reason;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
    

    public async Task<Response.CreateOrderResponse> CreateOrder(Request.CreateOrderRequest request)
    {
        var cusId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;

        var cusIdGuid = Guid.Parse(cusId!);

        var foodIdList = request.Foods.Select(x => x.FoodId).Distinct().ToList();

        var query = _dbContext.Foods.Where(x => foodIdList.Contains(x.Id));

        var foodCount = await query.CountAsync();

        if (foodCount != foodIdList.Count)
        {
            throw new Exception("Some food not found");
        }

        var result = await query.ToListAsync();

        decimal totalAmount = 0;

        foreach (var food in result)
        {
            var quality = request.Foods.First(x => x.FoodId == food.Id).Quantity;

            if (quality <= 0)
            {
                throw new Exception($"Quantity of product {food.Id} must be greater than 0");
            }

            totalAmount += quality * food.Price;
        }


        if (totalAmount <= 0)
        {
            throw new Exception("total amount must be greater than 0");
        }

        var order = new Order()
        {
            Id = Guid.NewGuid(),
            CustomerId = cusIdGuid,
            DeliveryAddress = request.DeliveryAddress,
            Name = request.Name,
            Notes = request.Notes,
            PaymentMethod = request.PaymentMethod,
            Status = "Pending",
            Discount = 0m,
            FinalPrice = totalAmount,
            ReviewerFee = 0m,
            OrderedAt = DateTimeOffset.UtcNow,
            PlatformFee = 0m,
            
        };

        _dbContext.Orders.Add(order);

        List<OrderDetail> orderDetails = new List<OrderDetail>();
        foreach (var food in result)
        {
            var quality = request.Foods.First(x => x.FoodId == food.Id).Quantity;

            var orderdt = new OrderDetail()
            {
                Id = Guid.NewGuid(),
                Name = food.Name,
                OrderId = order.Id,
                FoodId = food.Id,
                Quantity = quality,
                UnitPrice = food.Price,
            };
            orderDetails.Add(orderdt);
        }

        if (orderDetails.Any())
        {
            _dbContext.AddRange(orderDetails);
            await _dbContext.SaveChangesAsync();
        }
        
        string description = $"UGem-{order.Id}";

        var response = new Response.CreateOrderResponse()
        {
            OrderId = order.Id,
            TotalAmount = order.FinalPrice,
            BankName = "MBBank",
            BankAccount = "VQRQAIDAX4356",
            Description = description,
            QRCode = ""
        };

        string qrCode = $"https://qr.sepay.vn/img?" +
                        $"acc={response.BankAccount}&" +
                        $"bank={response.BankName}&" +
                        $"amount={(int)totalAmount}&" +
                        $"des={description}&" +
                        $"template=qronly";
        
        response.QRCode = qrCode;
        return response;
    }
    
     public async Task SepayWebhookHandler(Request.SepayWebhookRequest request)
    {
        var content = request.Content!
            .Replace(" ", "")
            .Replace("\n", "")
            .Replace("\r", "");

        var startIndex = content.IndexOf("UGem");

        if (startIndex == -1)
        {
            throw new Exception("UG code not found");
        }

        var description = content.Substring(startIndex, 36);

        var raw = description.Replace("UGem", "");

        Guid? orderId = null;

        if (raw.Length == 32)

        {
            var formatted = $"{raw.Substring(startIndex: 0, length: 8)}-" +
                                   $"{raw.Substring(startIndex: 8, length: 4)}-" +
                                   $"{raw.Substring(startIndex: 12, length: 4)}-" +
                                   $"{raw.Substring(startIndex: 16, length: 4)}-" +
                                   $"{raw.Substring(startIndex: 20, length: 12)}";

            if (Guid.TryParse(formatted, out var guid))
            {
                orderId = guid;
            }
        }
        else
        {
            throw new Exception("Invalid description format");
        }

        var query = _dbContext.Orders
            .Where(x => x.Id == orderId)
            .Include(x => x.OrderDetails);

        var order = await query.FirstOrDefaultAsync();

        if(order == null)
        {
            throw new Exception("Order not found");
        }

        if(order.Status != "Pending")
        {
            throw new Exception("Order already processed");
        }

        if(order.FinalPrice != request.TransferAmount)
        {
            order.Status = "Failed";

            _dbContext.Update(order);
            await _dbContext.SaveChangesAsync();

            throw new Exception("Invalid transfer amount");
        }

        order.Status = "Completed";
        _dbContext.Update(order);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Response.OrderResponse>> GetOrderListFromCustomerId()
    {
        var cusId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;

        var cusIdGuid = Guid.Parse(cusId!);

        var order = _dbContext.Orders.Where(x => x.CustomerId == cusIdGuid);

        var selectOrder = order.Select(x => new Response.OrderResponse()
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
        });

        var listOrder = await selectOrder.ToListAsync();

        return listOrder;
    }

    public async Task<List<Response.GetOrderDetailResponse>> GetOrderDetail(Guid orderId)
    {
        var orderDetail = _dbContext.OrderDetails.Where(x => x.OrderId == orderId);

        var selectOrder = orderDetail.Select(x => new Response.GetOrderDetailResponse()
        {
            Name = x.Name,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            Notes = x.Notes,
            OrderId = x.OrderId,
            FoodId = x.FoodId,
        });

        var listOrder = await selectOrder.ToListAsync();

        return listOrder;
    }
        public async Task ConfirmOrderReceived(Request.ConfirmOrderRequest request)
    {
        var customerId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;

        var cusIdGuid = Guid.Parse(customerId!);
        
        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Food)
                    .ThenInclude(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.CustomerId == cusIdGuid);

        if (order == null)
        {
            throw new  Exception("Order not found");
        }
        
        order.Status = "Completed";
        order.UpdatedAt = DateTimeOffset.UtcNow;

    
        var merchantId = order.OrderDetails
            .Select(od => od.Food.MerchantId)
            .FirstOrDefault();
        
        if (merchantId != Guid.Empty)
        {
            await _checkInService.CreateCheckIn(cusIdGuid, merchantId);
        }
        
        var userId  = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        var userIdGuid = Guid.Parse(userId!);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        
        var merchant = order.OrderDetails
            .Select(x => x.Food.Merchant)
            .FirstOrDefault();

        var notificationMerchant = new Notification()
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
        var customerId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;

        var cusIdGuid = Guid.Parse(customerId!);
        
        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.Food)
            .ThenInclude(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.CustomerId == cusIdGuid);

        if (order == null)
        {
            throw new  Exception("Order not found");
        }
        
        order.Status = "NotReceived";
    
        
        var userId  = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        var userIdGuid = Guid.Parse(userId!);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        
        var merchant = order.OrderDetails
            .Select(x => x.Food.Merchant)
            .FirstOrDefault();

        var notificationMerchant = new Notification()
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
}