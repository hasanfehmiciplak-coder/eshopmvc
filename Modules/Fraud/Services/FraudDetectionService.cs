using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Fraud.Rules;
using EShopMVC.Modules.Orders.Domain.Entities;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudDetectionService
    {
        private readonly IEnumerable<IFraudRule> _rules;

        public FraudDetectionService(IEnumerable<IFraudRule> rules)
        {
            _rules = rules;
        }

        public List<FraudReason> Check(Order order)
        {
            var list = new List<FraudReason>();

            if (order.TotalPrice > 50000)
                list.Add(FraudReason.HighAmount);

            return list;
        }
    }
}