using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShopMVC.Modules.Orders.Domain.Enums;
using System.Linq;

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
            var topRefundUsers = await _context.Refunds
                .GroupBy(x => x.OrderItem.OrderId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var userIds = topRefundUsers
                .Select(x => x.UserId.ToString())
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Email);

            var result = topRefundUsers.Select(x => new
            {
                Email = users.GetValueOrDefault(x.UserId.ToString(), "Unknown"),
                x.Count,
                x.Total
            }).ToList();

            // 🔴 Çok iade yapılan siparişler
            var riskyOrders = await _context.Refunds
                .GroupBy(x => x.OrderItemId)
                .Select(g => new
                {
                    OrderId = g.Key,
                    RefundCount = g.Count(),
                    RefundTotal = g.Sum(x => x.Amount)
                })
                .Where(x => x.RefundCount > 1)
                .OrderByDescending(x => x.RefundCount)
                .ToListAsync();

            ViewBag.TopRefundUsers = result;
            ViewBag.RiskyOrders = riskyOrders;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> RefundTrendChart()
        {
            var startDate = DateTime.Today.AddDays(-6);

            var data = await _context.Refunds
                .Where(x => x.CreatedAt >= startDate)
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(x => x.Amount)
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

            var totalRefunded = await _context.Refunds
                .SumAsync(x => x.Amount);

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