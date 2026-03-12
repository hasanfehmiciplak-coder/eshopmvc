using EShopMVC.Models;
using EShopMVC.Models.Fraud;

namespace EShopMVC.Areas.Admin.ViewModels
{
    public class AdminOrderIndexVM
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }

        public FraudSeverity? MaxFraudSeverity { get; set; }

        public bool HasHighFraud { get; set; }
        public bool RefundOverrideEnabled { get; set; }

        public bool HasMediumFraud { get; set; }

        public int PaymentAttemptCount { get; set; }
        public int SuccessfulPaymentCount { get; set; }
        public int FailedPaymentCount { get; set; }
    }
}