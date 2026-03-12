using EShopMVC.Models;

namespace EShopMVC.Services
{
    public interface IPaymentRefundGateway
    {
        Task<bool> RefundAsync(Refund refund);
    }
}