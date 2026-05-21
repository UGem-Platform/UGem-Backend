namespace UGem.Services.MonetizationService;

public interface IService
{
    Task HandlePaymentSuccess(Guid orderId);
    Task ProcessCompletedOrdersMissingMonetization(Guid? merchantId = null, Guid? reviewerId = null);
    Task ReprocessCompletedOrder(Guid orderId);
    Task HandleRefund(Guid orderId);
}