using Newtonsoft.Json;

namespace PaymentGatewayIntegration.Model
{
    public class KhaltiPaymentInitiateContentResponseModel
    {
        [JsonProperty("pidx")]
        public string SessionId { get; set; }

        [JsonProperty("payment_url")]
        public string PaymentUrl { get; set; }

        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
