using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Logs;
using EShopMVC.Modules.Orders.Domain.Refunds;

namespace EShopMVC.Areas.Admin.ViewModels.Admin
{
    public class FraudInvestigationVM
    {
        public Order Order { get; set; }

        public int RiskScore { get; set; }

        public List<PaymentLog> PaymentAttempts { get; set; }

        public List<Modules.Orders.Domain.Entities.Refund> Refunds { get; set; }

        public ICollection<FraudFlag> FraudFlags { get; set; }
    }
}