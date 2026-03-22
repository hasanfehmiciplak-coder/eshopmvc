using EShopMVC.Areas.Admin.ViewModels;
using EShopMVC.Areas.Admin.ViewModels.Admin;
using EShopMVC.Infrastructure.Caching;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Fraud.Repositories;
using EShopMVC.Modules.Fraud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FraudController : Controller
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IFraudFlagRepository _fraudFlagRepository;
    private readonly OrderRiskService _riskService;

    public FraudController(AppDbContext context,
        IMemoryCache cache,
        IFraudFlagRepository fraudFlagRepository,
        OrderRiskService riskService)
    {
        _context = context;
        _cache = cache;
        _fraudFlagRepository = fraudFlagRepository;
        _riskService = riskService;
    }

    public async Task<IActionResult> Index()
    {
        var flags = await _context.FraudFlags
            .Include(x => x.Order)
            .OrderByDescending(x => x.CreatedAt)
            .Take(50)
            .ToListAsync();

        return View(flags);
    }

    public async Task<IActionResult> HighRiskOrders()
    {
        var orders = await _context.FraudFlags
            .Where(x => x.Severity == FraudSeverity.High)
            .Include(x => x.Order)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> SuspiciousUsers()
    {
        var users = await _context.UserFraudScores
            .Where(x => x.Score > 70)
            .OrderByDescending(x => x.Score)
            .ToListAsync();

        return View(users);
    }

    public async Task<IActionResult> Heatmap()
    {
        var data = await _context.Orders
            .Where(x => x.IpAddress != null)
            .GroupBy(x => x.IpAddress)
            .Select(g => new
            {
                Ip = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(50)
            .ToListAsync();

        return View(data);
    }

    [HttpPost]
    public IActionResult Resolve(int id, string note)
    {
        var flag = _fraudFlagRepository.GetById(id);

        if (flag == null)
            return NotFound();

        flag.Resolve("Ignored by admin", "SYSTEM");
        flag.ResolvedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _fraudFlagRepository.Update(flag);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Metrics()
    {
        var totalAlerts = await _context.FraudAlerts.CountAsync();

        var openCases = await _context.FraudCases
            .Where(x => x.Status == EShopMVC.Modules.Fraud.Models.FraudCaseStatus.Open)
            .CountAsync();

        var highRiskOrders = await _context.FraudFlags
            .Where(x => x.Severity == FraudSeverity.High)
            .CountAsync();

        var totalOrders = await _context.Orders.CountAsync();

        var refundCount = await _context.Refunds.CountAsync();

        var refundRate = totalOrders == 0
            ? 0
            : (decimal)refundCount / totalOrders * 100;

        var model = new FraudMetricsVM
        {
            TotalAlerts = totalAlerts,
            OpenCases = openCases,
            HighRiskOrders = highRiskOrders,
            RefundAbuseRate = refundRate
        };

        return View(model);
    }

    public async Task<IActionResult> Investigation(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.PaymentLogs)
            .Include(o => o.PartialRefunds)
            .Include(o => o.FraudFlags)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound();

        var riskScore = await _riskService.CalculateRiskScore(order.Id);

        var vm = new FraudInvestigationVM
        {
            Order = order,
            RiskScore = riskScore,
            PaymentAttempts = order.PaymentLogs
                .OrderByDescending(p => p.CreatedAt)
                .ToList(),
            Refunds = order.PartialRefunds
                .OrderByDescending(r => r.CreatedAt)
                .ToList(),
            FraudFlags = order.FraudFlags
        };

        return View(vm);
    }

    public async Task<IActionResult> Dashboard()
    {
        var totalOrders = await _context.Orders.CountAsync();
        var totalRefunds = await _context.Refunds.CountAsync();
        var fraudFlags = await _context.FraudFlags.CountAsync();

        var refundRate = totalOrders == 0 ? 0 :
            (double)totalRefunds / totalOrders * 100;

        var fraudRate = totalOrders == 0 ? 0 :
            (double)fraudFlags / totalOrders * 100;

        ViewBag.TotalOrders = totalOrders;
        ViewBag.TotalRefunds = totalRefunds;
        ViewBag.FraudFlags = fraudFlags;
        ViewBag.RefundRate = Math.Round(refundRate, 2);
        ViewBag.FraudRate = Math.Round(fraudRate, 2);

        // Fraud Trend Chart
        var fraudTrend = await _context.FraudFlags
            .GroupBy(f => f.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        ViewBag.FraudTrendDates =
            fraudTrend.Select(x => x.Date.ToString("dd.MM")).ToList();

        ViewBag.FraudTrendCounts =
            fraudTrend.Select(x => x.Count).ToList();

        // Suspicious Orders
        var suspiciousOrders = await _context.Orders
            .Select(o => new
            {
                o.Id,
                UserEmail = _context.Users
                    .Where(u => u.Id == o.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefault(),

                RefundCount = _context.Refunds.Count(r => r.OrderItemId == o.Id),
                FraudCount = _context.FraudFlags.Count(f => f.OrderId == o.Id)
            })
            .OrderByDescending(x => x.FraudCount)
            .ThenByDescending(x => x.RefundCount)
            .Take(10)
            .ToListAsync();

        ViewBag.SuspiciousOrders = suspiciousOrders;

        return View();
    }

    public async Task<IActionResult> Case(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        // ✔ RefundLog buraya
        var refunds = await _context.Refunds
            .Where(r => r.OrderItemId == id)
            .ToListAsync();

        var paymentLogs = await _context.PaymentLogs
            .Where(p => p.OrderId == id)
            .ToListAsync();

        var vm = new OrderDetailViewModel
        {
            Order = order,
            Refunds = refunds,
            PaymentLogs = paymentLogs
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Ignore(int id)
    {
        var flag = await _context.FraudFlags.FindAsync(id);
        if (flag == null)
            return NotFound();

        flag.Resolve("Ignored by admin", "SYSTEM");
        flag.ResolvedAt = DateTime.UtcNow;
        flag.ResolutionNote = "Ignored by admin";

        await _context.SaveChangesAsync();

        // 🔥 DOĞRU CACHE INVALIDATION
        _cache.Remove(CacheKeys.OrderDetails(flag.OrderId));

        return RedirectToAction(
            "Details",
            "Order",
            new { area = "Admin", id = flag.OrderId }
        );
    }
}