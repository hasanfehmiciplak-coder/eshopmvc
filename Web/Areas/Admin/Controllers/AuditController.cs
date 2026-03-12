using EShopMVC.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AuditController : Controller
    {
        private readonly AppDbContext _context;

        public AuditController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string actionType = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (Request.Query.ContainsKey("today"))
            {
                var today = DateTime.Today;
                query = query.Where(x => x.CreatedAt >= today);
            }

            if (!string.IsNullOrEmpty(actionType))
            {
                query = query.Where(x => x.Action == actionType);
            }

            var logs = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(500)
                .ToListAsync();

            ViewBag.ActionType = actionType;

            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(string actionType = null, bool today = false)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(actionType))
                query = query.Where(x => x.Action == actionType);

            if (today)
                query = query.Where(x => x.CreatedAt >= DateTime.Today);

            var logs = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(5000) // güvenlik & performans
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Tarih,Email,Aksiyon,IP,UserAgent");

            foreach (var log in logs)
            {
                sb.AppendLine(
                    $"{log.CreatedAt:dd.MM.yyyy HH:mm}," +
                    $"{Escape(log.Email)}," +
                    $"{log.Action}," +
                    $"{log.IpAddress}," +
                    $"{Escape(log.UserAgent)}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(
                bytes,
                "text/csv",
                $"audit-log-{DateTime.Now:yyyyMMdd-HHmm}.csv"
            );
        }

        private string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }

        [HttpGet]
        public async Task<IActionResult> LoginFailChart()
        {
            var startDate = DateTime.Today.AddDays(-6);

            var data = await _context.AuditLogs
                .Where(x => x.Action == "LOGIN_FAIL" && x.CreatedAt >= startDate)
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Json(data.Select(x => new
            {
                date = x.Date.ToString("dd.MM"),
                count = x.Count
            }));
        }

        [HttpGet]
        public async Task<IActionResult> LoginCompareChart()
        {
            var startDate = DateTime.Today.AddDays(-6);

            var logs = await _context.AuditLogs
                .Where(x =>
                    (x.Action == "LOGIN_SUCCESS" || x.Action == "LOGIN_FAIL") &&
                    x.CreatedAt >= startDate)
                .GroupBy(x => new { x.CreatedAt.Date, x.Action })
                .Select(g => new
                {
                    Date = g.Key.Date,
                    Action = g.Key.Action,
                    Count = g.Count()
                })
                .ToListAsync();

            var days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i).Date)
                .ToList();

            var result = days.Select(d => new
            {
                date = d.ToString("dd.MM"),
                success = logs
                    .Where(x => x.Date == d && x.Action == "LOGIN_SUCCESS")
                    .Sum(x => x.Count),
                fail = logs
                    .Where(x => x.Date == d && x.Action == "LOGIN_FAIL")
                    .Sum(x => x.Count)
            });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> LoginFailByIpChart()
        {
            var startDate = DateTime.Today.AddDays(-7);

            var data = await _context.AuditLogs
                .Where(x =>
                    x.Action == "LOGIN_FAIL" &&
                    x.CreatedAt >= startDate &&
                    x.IpAddress != null)
                .GroupBy(x => x.IpAddress)
                .Select(g => new
                {
                    Ip = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10) // Top 10 IP
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> LoginFailByUserChart()
        {
            var startDate = DateTime.Today.AddDays(-7);

            var data = await _context.AuditLogs
                .Where(x =>
                    x.Action == "LOGIN_FAIL" &&
                    x.CreatedAt >= startDate &&
                    x.Email != null)
                .GroupBy(x => x.Email)
                .Select(g => new
                {
                    Email = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10) // en çok fail olan 10 kullanıcı
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> User(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Index");

            var logs = await _context.AuditLogs
                .Where(x => x.Email == email)
                .OrderByDescending(x => x.CreatedAt)
                .Take(500)
                .ToListAsync();

            ViewBag.Email = email;
            return View(logs);
        }
    }
}