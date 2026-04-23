using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.OrderService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public Task<List<Response.GetOrderListResponse>> GetOrdersList()
    {
        throw new NotImplementedException();
    }

    public async Task CreateOrder(Request.CreateOrderRequest request)
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
                throw new  Exception($"Quantity of product {food.Id} must be greater than 0");
            }
            
            totalAmount += quality * food.Price;
        }
        
        
        if (totalAmount <= 0)
        {
            throw new Exception("total amount must be greater than 0");
        }
        
        var order = new Order()
        {
            Id =  Guid.NewGuid(),
            CustomerId = cusIdGuid,
            DeliveryAddress = request.DeliveryAddress,
            Name = request.Name,
            Notes = request.Notes,
            PaymentMethod = request.PaymentMethod,
            Status = request.Status,
            Discount = request.Discount,
            FinalPrice = totalAmount,
            ReviewerFee = request.ReviewerFee,
            OrderedAt = DateTimeOffset.UtcNow,
            PlatformFee = request.PlatformFee,
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
    }

    public async Task<List<Response.OrderResponse>> GetOrderListFromCustomerId()
    {
        var cusId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;

        var cusIdGuid = Guid.Parse(cusId!);
        
        var order = _dbContext.Orders.Where(x => x.CustomerId == cusIdGuid);

        var isExist = await order.AnyAsync();

        if (!isExist)
        {
            throw new Exception("No orders");
        }

        var selectOrder = order.Select(x => new Response.OrderResponse()
        {
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

        var isExist = await orderDetail.AnyAsync();

        if (!isExist)
        {
            throw new Exception("No orders");
        }

        var selectOrder = orderDetail.Select(x => new Response.GetOrderDetailResponse()
        {
            Name = x.Name,
            Quantity =  x.Quantity,
            UnitPrice =  x.UnitPrice,
            Notes =  x.Notes,
            OrderId = x.OrderId,
            FoodId =  x.FoodId,
        });
        
        var listOrder = await selectOrder.ToListAsync();

        return listOrder;
    }
}