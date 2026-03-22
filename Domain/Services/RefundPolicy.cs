using EShopMVC.Modules.Orders.Domain.Entities;

namespace EShopMVC.Modules.Orders.Domain.Services
{
    public class RefundPolicy
    {
        public bool CanRefund(Order order)
        {
            if (!order.IsPaid)
                return false;

            if (order.CreatedAt < DateTime.UtcNow.AddDays(-14))
                return false;

            return true;
        }
    }
}