using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Shared.Specifications;

namespace EShopMVC.Modules.Orders.Specifications
{
    public class HighRiskOrdersSpecification : BaseSpecification<Order>
    {
        public HighRiskOrdersSpecification()
            : base(o => true) // geçici
        {
        }
    }
}