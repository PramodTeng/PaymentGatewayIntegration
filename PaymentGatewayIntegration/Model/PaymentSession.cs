namespace PaymentGatewayIntegration.Model
{
    public class PaymentSession: PaymentIntentRequestModel
    {
        public string SessionId { get; set; }
        public Dictionary<string, string> FormData { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
    }
}
