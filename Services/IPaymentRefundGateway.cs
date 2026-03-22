using EShopMVC.Models;
using EShopMVC.Modules.Orders.Domain.Entities;

namespace EShopMVC.Services
{
    public interface IPaymentRefundGateway
    {
        Task<bool> RefundAsync(Refund refund);
    }
}