using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UGem.Repositories;
using UGem.Repositories.Entity;
using UGem.Services.MailService;

namespace UGem.Services.CustomerService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly MailService.IService _mailService;
    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext, MailService.IService mailService)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _mailService = mailService;
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
    public async Task<string> CreateCustomer(Request.RegisterCustomerRequest request)
    {
        var existingUserQuery = _dbContext.Users.Where(x => x.Email == request.Email);
        bool isExistUser = await existingUserQuery.AnyAsync();
        if (isExistUser)
        {
            throw new Exception("User Already Exist with this mail");
        }
        var allowedRoles = new[] { "Customer", "Merchant" };
        if (!allowedRoles.Contains(request.Role))
        {
            throw new Exception("Invalid role. Only 'Customer' or 'Merchant' are allowed");
        }

        var user = new Repositories.Entity.User
        {
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = request.HashedPassword,
            PhoneNumber = request.PhoneNumber,
            AvatarUrl = request.AvatarUrl,
            Role = request.Role
        };
        _dbContext.Users.Add(user);
        var result = await _dbContext.SaveChangesAsync();
        if (result > 0)
        {
            var customer = new Repositories.Entity.Customer
            {
                UserId = user.Id,
            };
            _dbContext.Customers.Add(customer);
            var customerResult = await _dbContext.SaveChangesAsync();
            await _mailService.SendMail(new MailContext()
            {
                To = request.Email,
                Subject = "Welcome to UGem!",
                Body = $"Dear {request.FullName} ,\n\n" +
                       "Thank you for registering as a Customer on UGem."
            });
            if (customerResult > 0) return "Add Customer successful";
        }

        return "Fail to add Customer";
    }
}