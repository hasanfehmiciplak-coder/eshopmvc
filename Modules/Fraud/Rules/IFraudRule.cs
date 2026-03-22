using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Orders.Domain.Entities;

namespace EShopMVC.Modules.Fraud.Rules
{
    public interface IFraudRule
    {
        FraudReason? Check(Order order);
    }
}