using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Shared.Specifications;

namespace EShopMVC.Modules.Orders.Specifications
{
    public class RefundableOrdersSpecification : BaseSpecification<Order>
    {
        public RefundableOrdersSpecification()
 : base(o => o.IsPaid) // minimum şart
        {
        }
    }
}