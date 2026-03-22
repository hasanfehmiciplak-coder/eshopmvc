using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Orders.Specifications;
using EShopMVC.Shared.Specifications;
using EShopMVC.Web.Areas.Admin.ViewModels;

using EShopMVC.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Modules.Orders.Queries
{
    public class OrdersQueryService
    {
        private readonly AppDbContext _context;

        public OrdersQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderCancelRequestVM>> GetCancelRequests()
        {
            var spec = new CancelRequestOrdersSpecification();

            var query = SpecificationEvaluator.GetQuery(
                _context.Orders.AsNoTracking(),
                spec);

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderCancelRequestVM
                {
                    Id = o.Id,
                    CustomerEmail = "", // geçici
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice
                })
                .ToListAsync();
        }

        public async Task<List<EShopMVC.Web.ViewModels.OrderListVM>> GetHighRiskOrders()
        {
            var spec = new HighRiskOrdersSpecification();

            var query = SpecificationEvaluator.GetQuery(
                _context.Orders.AsNoTracking(),
                spec);

            return await query
                .Select(o => new EShopMVC.Web.ViewModels.OrderListVM
                {
                    Id = o.Id,
                    TotalPrice = o.TotalPrice,
                    CreatedDate = o.OrderDate
                })
                .ToListAsync();
        }
    }
}