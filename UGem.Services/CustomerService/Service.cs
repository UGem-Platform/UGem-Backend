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
    
    

}