using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RefundFraudController : Controller
    {
        private readonly AppDbContext _context;

        public RefundFraudController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 🔴 Çok iade yapan kullanıcılar
            var topRefundUsers = await _context.PartialRefunds
                .Include(x => x.Order)
                .ThenInclude(o => o.User)
                .GroupBy(x => x.Order.User.Email)
                .Select(g => new
                {
                    Email = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(x => x.RefundAmount)
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // 🔴 Çok iade yapılan siparişler
            var riskyOrders = await _context.PartialRefunds
                .GroupBy(x => x.OrderId)
                .Select(g => new
                {
                    OrderId = g.Key,
                    RefundCount = g.Count(),
                    RefundTotal = g.Sum(x => x.RefundAmount)
                })
                .Where(x => x.RefundCount > 1)
                .OrderByDescending(x => x.RefundCount)
                .ToListAsync();

            ViewBag.TopRefundUsers = topRefundUsers;
            ViewBag.RiskyOrders = riskyOrders;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> RefundTrendChart()
        {
            var startDate = DateTime.Today.AddDays(-6);

            var data = await _context.PartialRefunds
                .Where(x => x.CreatedAt >= startDate)
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(x => x.RefundAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Json(data.Select(x => new
            {
                date = x.Date.ToString("dd.MM"),
                count = x.Count,
                total = x.Total
            }));
        }

        [HttpGet]
        public async Task<IActionResult> RefundRatio()
        {
            var totalPaid = await _context.Orders
                .Where(x => x.Status != OrderStatus.Cancelled)
                .SumAsync(x => x.TotalPrice);

            var totalRefunded = await _context.PartialRefunds
                .SumAsync(x => x.RefundAmount);

            var ratio = totalPaid == 0 ? 0 : (totalRefunded / totalPaid) * 100;

            return Json(new
            {
                totalPaid,
                totalRefunded,
                ratio = Math.Round(ratio, 2)
            });
        }
    }
}