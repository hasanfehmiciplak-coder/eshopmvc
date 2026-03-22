using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Shared.Specifications;
using EShopMVC.Modules.Orders.Domain.Enums;

namespace EShopMVC.Modules.Orders.Specifications
{
    public class CancelRequestOrdersSpecification : BaseSpecification<Order>
    {
        public CancelRequestOrdersSpecification()
               : base(o => o.Status == OrderStatus.CancelRequested)
        {
        }
    }
}