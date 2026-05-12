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

    if (order.Status != Request.OrderStatus.BillConfirmed.ToString())
        throw new InvalidOperationException("Cash payment can only be confirmed after bill is confirmed");

    var merchantId = order.OrderDetails
        .Select(od => od.Food.MerchantId)
        .FirstOrDefault();

    if (merchantId == Guid.Empty)
        throw new KeyNotFoundException("Merchant not found");

    order.Status = Request.OrderStatus.Completed.ToString();
    order.UpdatedAt = DateTimeOffset.UtcNow;

    await _checkInService.CreateCheckIn(order.CustomerId, merchantId);

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
}