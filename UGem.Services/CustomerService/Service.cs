using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;

namespace UGem.Services.CustomerService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task<Response.GetCustomerDetailsResponse> GetProfile()
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        
        var userIdGuid = Guid.Parse(userId!);
        
        var customer = await _dbContext.Customers
            .Include(customer => customer.User)
            .FirstOrDefaultAsync(x => x.UserId == userIdGuid);
        
        if (customer == null)
        {
            throw new Exception("Customer not found");
        }
        
        var result = new Response.GetCustomerDetailsResponse()
        {
            Id = customer.UserId,
            Name = customer.User.FullName,
            Email = customer.User.Email,
            PhoneNumber = customer.User.PhoneNumber,
            Role = customer.User.Role,
        };

        return result;
    }

    public Task<Response.GetCustomerDetailsResponse> GetCustomer(int id)
    {
        throw new NotImplementedException();
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
    
        
        var userId  = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        var userIdGuid = Guid.Parse(userId!);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        
        var merchantId = order.OrderDetails
            .Select(x => x.Food.Merchant.UserId)
            .FirstOrDefault();

        var notificationMerchant = new Notification()
        {
            UserId = merchantId,
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
        
        var merchantId = order.OrderDetails
            .Select(x => x.Food.Merchant.UserId)
            .FirstOrDefault();

        var notificationMerchant = new Notification()
        {
            UserId = merchantId,
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