using PaymentGatewayIntegration.Model;

namespace PaymentGatewayIntegration.Services
{
    public interface IPaymentGatewayService
    {
        Task<PaymentSession> CreateEsewaCheckoutSession(PaymentIntentRequestModel model);
        Task<PaymentSession> CreateKhaltiCheckoutSession(PaymentIntentRequestModel model);
        Task<PaymentSession> CreateStripeCheckoutSession(PaymentIntentRequestModel model);
    }
}
