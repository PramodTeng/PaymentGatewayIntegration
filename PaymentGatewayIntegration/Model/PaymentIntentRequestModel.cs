using System.ComponentModel.DataAnnotations;

namespace PaymentGatewayIntegration.Model
{
    public class PaymentIntentRequestModel
    {
        /// <summary>
        /// Get or set Amount for Payment Intent
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a positive value for the amount")]
        public long Amount { get; set; }

        public PaymentGatewayEnum PaymentGatewayEnum { get; set; }
    }
}
