using EShopMVC.Areas.Admin.ViewModels;
using EShopMVC.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FraudGraphController : Controller
    {
        private readonly FraudGraphService _graphService;
        private readonly AppDbContext _context;

        public FraudGraphController(FraudGraphService graphService,
            AppDbContext _context)
        {
            _graphService = graphService;
            this._context = _context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _graphService.GetGraphData();
            return View(data);
        }

        public async Task<IActionResult> User(string userId)
        {
            var orders = await _context.Orders
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var refunds = await _context.Refunds
                .Where(x => orders.Select(o => o.Id).Contains(x.OrderItemId))
                .CountAsync();

            var ips = orders
                .Where(x => x.IpAddress != null)
                .Select(x => x.IpAddress)
                .Distinct()
                .ToList();

            var model = new FraudGraphVM
            {
                UserId = userId,
                Orders = orders.Select(x => x.Id).ToList(),
                IpAddresses = ips,
                RefundCount = refunds
            };

            return View(model);
        }
    }
}