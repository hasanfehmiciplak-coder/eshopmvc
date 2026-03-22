using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Domain.Entities;

namespace EShopMVC.Modules.Fraud.Rules
{
    public class HighAmountRule : IFraudRule
    {
        public FraudReason? Check(Order order)
        {
            if (order.TotalPrice > 50000)
            {
                return FraudReason.HighAmount;
            }

            return null;
        }
    }
}