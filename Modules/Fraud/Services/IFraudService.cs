using EShopMVC.Models;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Refunds;

namespace EShopMVC.Modules.Fraud.Services
{
    public interface IFraudService
    {
        void FlagRefund(FraudFlag flag);

        FraudFlag? EvaluateRefund(
            Order order,
            Orders.Domain.Entities.Refund refund);

        Task ResolveRefundFraudAsync(int orderId);
    }
}