using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PaymentGatewayIntegration.Configuration;
using PaymentGatewayIntegration.Model;
using RestSharp;
using Stripe;
using Stripe.Checkout;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace PaymentGatewayIntegration.Services
{
    public class PaymentServices : IPaymentGatewayService
    {
        private readonly ILogger<PaymentServices> _logger;
        private readonly ApplicationInfo _config;
        public PaymentServices(
            IOptions<ApplicationInfo> config,
            ILogger<PaymentServices> logger)
        {
            _config = config.Value;
            _logger = logger;
        }
        public async Task<PaymentSession> CreateEsewaCheckoutSession(PaymentIntentRequestModel model)
        {
            var result = new PaymentSession();

            try
            {
                result.SessionId = DateTime.UtcNow.Ticks.ToString();
                string dataToHash = $"total_amount={model.Amount},transaction_uuid={result.SessionId},product_code={_config.ESEWA_MERCHANT_CODE}";
                var signature = GenerateHMACSHA256Signature(dataToHash, _config.ESEWA_SECRET_KEY);

                result.FormData = new Dictionary<string, string>
        {
            { "amount", model.Amount.ToString() },
            { "tax_amount", "0" },
            { "total_amount", model.Amount.ToString() },
            { "transaction_uuid", result.SessionId },
            { "product_code", _config.ESEWA_MERCHANT_CODE },
            { "product_service_charge", "0" },
            { "product_delivery_charge", "0" },
            { "success_url", _config.HOST_URL + "/Verify" },
            { "failure_url", _config.HOST_URL + "/Cancel?session_id=" + result.SessionId },
            { "signed_field_names", "total_amount,transaction_uuid,product_code" },
            { "signature", signature },
            { "url", _config.ESEWA_API_URL + "/main/v2/form" }
        };

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating checkout session: {Message}", ex.Message);
                result.IsSuccess = false;
                result.Message = "An error occurred while creating the checkout session.";
            }

            return result;
        }


        private static string GenerateHMACSHA256Signature(string message, string secret)
        {
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacSha256 = new HMACSHA256(keyByte))
            {
                byte[] hashMessage = hmacSha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashMessage);
            }
        }

        public async Task<PaymentSession> CreateKhaltiCheckoutSession(PaymentIntentRequestModel model)
        {
            var result = new PaymentSession();
            try
            {
                var url = $"{_config.KHALTI_API_URL}/epayment/initiate/";
                result.SessionId = DateTime.UtcNow.Ticks.ToString();
                result.Amount = model.Amount;

                var payload = new
                {
                    return_url = $"{_config.HOST_URL}/verify/khalti",
                    website_url = _config.HOST_URL,
                    amount = model.Amount * 100, // Convert amount to paisa
                    purchase_order_id = result.SessionId,
                    purchase_order_name = "Donation",
                    customer_info = new
                    {
                        name = "Test User",
                        email = "test@khalti.com",
                        phone = "9800000000"
                    }
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Key {_config.KHALTI_SECRET_KEY}");

                    var response = await client.PostAsync(url, content).ConfigureAwait(false);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"Payload sent to Khalti: {jsonPayload}");
                    _logger.LogInformation($"Khalti API Response: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseObj = JsonConvert.DeserializeObject<KhaltiPaymentInitiateContentResponseModel>(responseContent);
                        result.IsSuccess = true;
                        result.Url = responseObj.PaymentUrl;
                    }
                    else
                    {
                        _logger.LogError($"Khalti API returned an error: {responseContent}");
                        result.IsSuccess = false;
                        result.Message = "Failed to create Khalti checkout session.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in CreateKhaltiCheckoutSession: {ex.Message}", ex);
                result.IsSuccess = false;
                result.Message = "An error occurred while creating the Khalti checkout session.";
            }
            return result;
        }


        public async Task<PaymentSession> CreateStripeCheckoutSession(PaymentIntentRequestModel model)
        {
            var result = new PaymentSession();
            StripeConfiguration.ApiKey = _config.STRIPE_SECRET_KEY;

            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "npr", // Update this to the appropriate currency code
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Donation", // You can update this to a generic name
                        },
                        UnitAmount = model.Amount * 100, // Amount in cents
                    },
                    Quantity = 1,
                },
            },
                    Mode = "payment",
                    SuccessUrl = _config.HOST_URL + "/verify/stripe?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = _config.HOST_URL + "/cancel",
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options).ConfigureAwait(false);

                result.Url = session.Url;
                result.SessionId = session.Id;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating checkout session for Stripe payment.");
                result.IsSuccess = false;
                result.Message = "An error occurred while creating the checkout session.";
            }

            return result;
        }
    }
}
