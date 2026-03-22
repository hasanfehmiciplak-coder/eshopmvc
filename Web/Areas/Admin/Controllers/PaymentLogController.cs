using EShopMVC.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PaymentLogController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentLogController(AppDbContext context)
        {
            _context = context;
        }

        // 📄 Liste
        public IActionResult Index(string status)
        {
            var query = _context.PaymentLogs
                .Include(p => p.Order)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
                ViewBag.StatusFilter = status;
            }

            var logs = query
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(logs);
        }

        // 🔍 Detay
        public IActionResult Detail(int id)
        {
            var log = _context.PaymentLogs
                .Include(p => p.Order)
                .FirstOrDefault(p => p.Id == id);

            if (log == null)
                return NotFound();

            return View(log);
        }
    }
}