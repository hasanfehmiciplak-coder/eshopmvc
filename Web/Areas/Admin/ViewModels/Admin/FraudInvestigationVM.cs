using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Models;
using EShopMVC.Modules.Payments.Models;

namespace EShopMVC.Areas.Admin.ViewModels.Admin
{
    public class FraudInvestigationVM
    {
        public Order Order { get; set; }

        public int RiskScore { get; set; }

        public List<PaymentLog> PaymentAttempts { get; set; }

        public List<PartialRefund> Refunds { get; set; }

        public ICollection<FraudFlag> FraudFlags { get; set; }
    }
}