using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Models;

namespace EShopMVC.Modules.Fraud.Services
{
    public interface IFraudService
    {
        void FlagRefund(FraudFlag flag);

        FraudFlag? EvaluateRefund(
            Order order,
            Refund refund);

        Task ResolveRefundFraudAsync(int orderId);
    }
}