using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentGatewayIntegration.Model;
using PaymentGatewayIntegration.Services;

namespace PaymentGatewayIntegration.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IPaymentGatewayService _paymentGatewayService;

        public IndexModel(
            ILogger<IndexModel> logger,
            IPaymentGatewayService paymentGatewayService
            )
        {
            _logger = logger;
            _paymentGatewayService = paymentGatewayService;
        }
        [BindProperty]
        public PaymentIntentRequestModel PaymentIntent { get; set; }
        public PaymentSession PaymentSession { get; set; }

        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPostAsync()
        {
         
            if (PaymentIntent == null || PaymentIntent.Amount <= 0)
            {
                TempData["ErrorMessage"] = "Invalid payment details.";
                return Page();
            }

            try
            {
                switch (PaymentIntent.PaymentGatewayEnum)
                {
                    case PaymentGatewayEnum.Esewa:
                        PaymentSession = await _paymentGatewayService.CreateEsewaCheckoutSession(PaymentIntent);
                        break;

                    case PaymentGatewayEnum.Khalti:
                        PaymentSession = await _paymentGatewayService.CreateKhaltiCheckoutSession(PaymentIntent);
                        break;

                    case PaymentGatewayEnum.Stripe:
                        PaymentSession = await _paymentGatewayService.CreateStripeCheckoutSession(PaymentIntent);
                        break;

                    default:
                        TempData["ErrorMessage"] = "Unsupported payment gateway.";
                        return Page();
                }

                if (PaymentSession.IsSuccess)
                {
                    return Page();
                }
                else
                {
                    TempData["ErrorMessage"] = PaymentSession.Message;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during payment processing: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An error occurred during payment processing.";
                return Page();
            }
        }
    }
}
