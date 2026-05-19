namespace UGem.Services.MonetizationService;

public interface IService
{
    Task HandlePaymentSuccess(Guid orderId);
    Task HandleRefund(Guid orderId);
}
