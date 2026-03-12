using EShopMVC.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderLogsController : Controller
    {
        private readonly AppDbContext _context;

        public OrderLogsController(AppDbContext context)
        {
            _context = context;
        }

        // 📜 Sipariş Logları
        public async Task<IActionResult> Index(int orderId)
        {
            var logs = await _context.OrderLogs
                .Where(l => l.OrderId == orderId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            ViewBag.OrderId = orderId;
            return View(logs);
        }
    }
}