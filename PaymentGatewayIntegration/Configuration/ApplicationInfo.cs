namespace PaymentGatewayIntegration.Configuration
{
    public class ApplicationInfo
    {

        public string HOST_URL { get; set; }  
        public string KHALTI_SECRET_KEY { get; set; }

        public string KHALTI_API_URL { get; set; }

        public string ESEWA_SECRET_KEY { get; set; }

        public string ESEWA_API_URL { get; set; }

        public string ESEWA_MERCHANT_CODE { get; set; }

        public string STRIPE_SECRET_KEY { get; set; }

    }
}
