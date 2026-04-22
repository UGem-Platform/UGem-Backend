using Microsoft.EntityFrameworkCore;
using UGem.Repositories;

namespace UGem.Services.OrderService;

public class Service:IService
{
    private readonly AppDbContext _dbContext;
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<List<Response.GetOrderListResponse>> GetOrdersList()
    {
        var selectQuery = _dbContext.Orders.Select(x => new Response.GetOrderListResponse
        {
            Name = x.Name,
            DeliveryAddress = x.DeliveryAddress,
            PaymentMethod = x.PaymentMethod,
            Status = x.Status,
            CustomerName = x.Customer.User.FullName
        });
        var resultList = await selectQuery.ToListAsync();
        return resultList;
    }
}