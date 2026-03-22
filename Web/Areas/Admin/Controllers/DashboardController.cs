using EShopMVC.Areas.Admin.ViewModels.Dashboard;
using EShopMVC.Areas.Admin.ViewModels.Orders;
using EShopMVC.Helpers;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Analytics.Services;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Fraud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;
    private readonly OrderRiskService _riskService;

    public DashboardController(
        DashboardService dashboardService,
        OrderRiskService riskService)
    {
        _dashboardService = dashboardService;
        _riskService = riskService;
    }

    public async Task<IActionResult> Index()
    {
        var vm = await _dashboardService.GetDashboard();

        ViewBag.RiskyOrders = await _riskService.GetRiskyOrders();

        return View(vm);
    }
}

//    private readonly AppDbContext _context;
//    private readonly OrderRiskService _riskService;
//    private readonly FraudScoreService _fraudScoreService;
//    private readonly IMemoryCache _cache;
//    private readonly AnalyticsService _analytics;

//    public DashboardController(AppDbContext context, IMemoryCache cache, OrderRiskService riskService, FraudScoreService fraudScoreService, AnalyticsService analytics)
//    {
//        _context = context;
//        _cache = cache;
//        _riskService = riskService;
//        _fraudScoreService = fraudScoreService;
//        _analytics = analytics;
//    }

//    private const string DashboardCacheKey = "admin_dashboard_summary";

//    public async Task<IActionResult> Index()
//    {
//        var today = DateTime.Today;

//        // ⚠ Riskli siparişler cache dışında
//        var riskyOrders = await _riskService.GetRiskyOrders();
//        ViewBag.RiskyOrders = riskyOrders;

//        if (!_cache.TryGetValue(DashboardCacheKey, out AdminDashboardViewModel vm))
//        {
//            // 📅 Son 7 gün listesi
//            var last7Days = Enumerable.Range(0, 7)
//                .Select(i => today.AddDays(-i))
//                .OrderBy(d => d)
//                .ToList();

//            // 📊 Fraud chart (TEK QUERY)
//            var fraudData = await _context.FraudFlags
//                .Where(f => f.CreatedAt >= today.AddDays(-7))
//                .GroupBy(f => f.CreatedAt.Date)
//                .Select(g => new
//                {
//                    Day = g.Key,
//                    Count = g.Count()
//                })
//                .ToListAsync();

//            var fraudLast7Days = last7Days
//                .Select(day => new DailyCountVM
//                {
//                    Day = day.ToString("dd.MM"),
//                    Count = fraudData
//                        .FirstOrDefault(x => x.Day == day)?.Count ?? 0
//                })
//                .ToList();

//            // 📊 Refund chart (TEK QUERY)
//            var refundData = await _context.PartialRefunds
//                .Where(r => r.CreatedAt >= today.AddDays(-7))
//                .GroupBy(r => r.CreatedAt.Date)
//                .Select(g => new
//                {
//                    Day = g.Key,
//                    Count = g.Count()
//                })
//                .ToListAsync();

//            var refundLast7Days = last7Days
//                .Select(day => new DailyCountVM
//                {
//                    Day = day.ToString("dd.MM"),
//                    Count = refundData
//                        .FirstOrDefault(x => x.Day == day)?.Count ?? 0
//                })
//                .ToList();

//            // ❌ Failed payments
//            var failedPaymentCount = await _context.PaymentLogs
//                .CountAsync(p => p.PaymentStatus != "SUCCESS");

//            // 🛑 Son fraudlar
//            var recentFrauds = await _context.FraudFlags
//                .OrderByDescending(f => f.CreatedAt)
//                .Take(5)
//                .Select(f => new OrderTimelineItemVM
//                {
//                    Date = f.CreatedAt,
//                    Type = "Fraud",
//                    Title = f.RuleCode,
//                    Description = f.Description
//                })
//                .ToListAsync();

//            // 💸 Son refundlar
//            var recentRefunds = await _context.PartialRefunds
//                .OrderByDescending(r => r.CreatedAt)
//                .Take(5)
//                .Select(r => new OrderTimelineItemVM
//                {
//                    Date = r.CreatedAt,
//                    Type = "Refund",
//                    Title = "Partial Refund",
//                    Description = $"{r.RefundAmount} ₺"
//                })
//                .ToListAsync();

//            // 💳 Son ödemeler
//            var recentPayments = await _context.PaymentLogs
//                .OrderByDescending(p => p.CreatedAt)
//                .Take(5)
//                .Select(p => new OrderTimelineItemVM
//                {
//                    Date = p.CreatedAt,
//                    Type = "Payment",
//                    Title = p.PaymentStatus,
//                    Description = p.PaymentStatus == "SUCCESS"
//                        ? "Payment successful"
//                        : p.ErrorMessage
//                })
//                .ToListAsync();

//            // 👤 Riskli kullanıcılar
//            var highRiskUsers = await _context.UserFraudScores
//                .CountAsync(x => x.RiskLevel == "High");

//            var mediumRiskUsers = await _context.UserFraudScores
//                .CountAsync(x => x.RiskLevel == "Medium");

//            var lowRiskUsers = await _context.UserFraudScores
//                .CountAsync(x => x.RiskLevel == "Low");

//            // 🧠 Dashboard ViewModel
//            vm = new AdminDashboardViewModel
//            {
//                FraudLast7Days = fraudLast7Days,
//                RefundLast7Days = refundLast7Days,

//                RecentActivities = recentFrauds
//                    .Concat(recentRefunds)
//                    .Concat(recentPayments)
//                    .OrderByDescending(x => x.Date)
//                    .Take(10)
//                    .ToList(),

//                ActiveFraudCount = await _context.FraudFlags
//                    .CountAsync(f => !f.IsResolved),

//                HighFraudCount = await _context.FraudFlags
//                    .CountAsync(f => !f.IsResolved && f.Severity == FraudSeverity.High),

//                TodayRefundCount = await _context.PartialRefunds
//                    .CountAsync(r => r.CreatedAt >= today),

//                TotalRefundAmount = await _context.PartialRefunds
//                    .SumAsync(r => (decimal?)r.RefundAmount) ?? 0,

//                FailedPaymentCount = failedPaymentCount,

//                HighRiskUserCount = highRiskUsers,
//                MediumRiskUserCount = mediumRiskUsers,
//                LowRiskUserCount = lowRiskUsers
//            };

//            // 🧠 Cache
//            _cache.Set(
//                DashboardCacheKey,
//                vm,
//                new MemoryCacheEntryOptions
//                {
//                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
//                });

//            var topFraudRules = await _context.FraudFlags
//                .GroupBy(f => f.RuleCode)
//                .Select(g => new
//                {
//                    Rule = g.Key,
//                    Count = g.Count()
//                })
//                .OrderByDescending(x => x.Count)
//                .Take(5)
//                .ToListAsync();

//            // 📊 Son 30 gün fraud trend
//            var last30Days = Enumerable.Range(0, 30)
//                .Select(i => DateTime.Today.AddDays(-i))
//                .OrderBy(d => d)
//                .ToList();

//            var fraudTrendData = await _context.FraudFlags
//                .Where(f => f.CreatedAt >= DateTime.Today.AddDays(-30))
//                .GroupBy(f => f.CreatedAt.Date)
//                .Select(g => new
//                {
//                    Day = g.Key,
//                    Count = g.Count()
//                })
//                .ToListAsync();

//            var fraudTrend = last30Days
//                .Select(day => new DailyCountVM
//                {
//                    Day = day.ToString("dd.MM"),
//                    Count = fraudTrendData
//                        .FirstOrDefault(x => x.Day == day)?.Count ?? 0
//                })
//                .ToList();

//            var openCases = await _context.FraudCases
//                    .Where(x => x.Status == EShopMVC.Modules.Fraud.Models.FraudCaseStatus.Open)
//                    .OrderByDescending(x => x.CreatedAt)
//                    .Take(5)
//                    .ToListAsync();

//            ViewBag.OpenFraudCases = openCases;

//            ViewBag.FraudTrend = fraudTrend;

//            ViewBag.TopFraudRules = topFraudRules;

//            var totalOrders = await _context.Orders.CountAsync();

//            var fraudOrders = await _context.FraudFlags
//                .Select(x => x.OrderId)
//                .Distinct()
//                .CountAsync();

//            var blockedOrders = await _context.Orders
//                .CountAsync(x => x.Status == OrderStatus.Blocked);

//            var velocityAlerts = await _context.FraudFlags
//                .CountAsync(x => x.RuleCode == "PAYMENT_VELOCITY");

//            var fraudRate = totalOrders == 0
//                ? 0
//                : (double)fraudOrders / totalOrders * 100;

//            ViewBag.FraudRate = fraudRate;
//            ViewBag.BlockedOrders = blockedOrders;
//            ViewBag.VelocityAlerts = velocityAlerts;
//        }

//        return View(vm ?? new AdminDashboardViewModel());
//    }

//    [HttpGet]
//    public IActionResult GetOrderStatusStats()
//    {
//        var data = _context.Orders
//            .GroupBy(o => o.Status)
//            .Select(g => new
//            {
//                Status = g.Key.ToString(),
//                Count = g.Count()
//            })
//            .ToList();

//        return Json(data);
//    }

//    [Authorize(Roles = "Admin")]
//    [HttpGet]
//    public IActionResult GetAuditStats()
//    {
//        if (!_cache.TryGetValue(DashboardCacheKeys.AuditStats, out List<object> data))
//        {
//            data = _context.AuditLogs
//                .Where(x => x.Action == "LoginFailed")
//                .GroupBy(x => x.CreatedAt.Date)
//                .OrderBy(g => g.Key)
//                .Select(g => new
//                {
//                    date = g.Key.ToString("dd.MM"),
//                    count = g.Count()
//                })
//                .Cast<object>()
//                .ToList();

//            _cache.Set(
//                DashboardCacheKeys.AuditStats,
//                data,
//                TimeSpan.FromMinutes(5)
//            );
//        }

//        return Json(data);
//    }

//    [Authorize(Roles = "Admin")]
//    [HttpGet]
//    public IActionResult GetIpAuditStats()
//    {
//        if (!_cache.TryGetValue(DashboardCacheKeys.IpAuditStats, out List<object> data))
//        {
//            data = _context.AuditLogs
//                .Where(x => x.Action == "LoginFailed")
//                .GroupBy(x => x.IpAddress)
//                .OrderByDescending(g => g.Count())
//                .Take(10)
//                .Select(g => new { ip = g.Key, count = g.Count() })
//                .Cast<object>()
//                .ToList();

//            _cache.Set(DashboardCacheKeys.IpAuditStats, data, TimeSpan.FromMinutes(5));
//        }
//        return Json(data);
//    }

//    [Authorize(Roles = "Admin")]
//    [HttpGet]
//    public IActionResult GetUserAuditStats()
//    {
//        if (!_cache.TryGetValue(DashboardCacheKeys.UserAuditStats, out List<object> data))
//        {
//            data = _context.AuditLogs
//                .Where(x => x.Action == "LoginFailed" && x.Email != null)
//                .GroupBy(x => x.Email)
//                .OrderByDescending(g => g.Count())
//                .Take(10)
//                .Select(g => new { user = g.Key, count = g.Count() })
//                .Cast<object>()
//                .ToList();

//            _cache.Set(DashboardCacheKeys.UserAuditStats, data, TimeSpan.FromMinutes(5));
//        }

//        return Json(data);
//    }

//    [Authorize(Roles = "Admin")]
//    [HttpGet]
//    public IActionResult GetAuditCompareStats()
//    {
//        if (!_cache.TryGetValue(DashboardCacheKeys.AuditCompareStats, out List<object> data))
//        {
//            data = _context.AuditLogs
//                .GroupBy(x => x.CreatedAt.Date)
//                .OrderBy(g => g.Key)
//                .Select(g => new
//                {
//                    date = g.Key.ToString("dd.MM"),
//                    success = g.Count(x => x.Action == "LoginSuccess"),
//                    fail = g.Count(x => x.Action == "LoginFailed")
//                })
//                .Cast<object>()
//                .ToList();

//            _cache.Set(DashboardCacheKeys.AuditCompareStats, data, TimeSpan.FromMinutes(5));
//        }

//        return Json(data);
//    }

//    [Authorize(Roles = "Admin")]
//    [HttpGet]
//    public IActionResult GetRefundFraudStats()
//    {
//        if (!_cache.TryGetValue(DashboardCacheKeys.RefundFraudStats, out object[] data))
//        {
//            data = new[]
//            {
//        new { label = "Fraud", count = _context.FraudFlags.Count(x => !x.IsResolved) },
//        new { label = "Refund", count = _context.PartialRefunds.Count() },
//        new { label = "Clean", count = _context.Orders.Count(o => !o.FraudFlags.Any()) }
//    };

//            _cache.Set(DashboardCacheKeys.RefundFraudStats, data, TimeSpan.FromMinutes(5));
//        }

//        return Json(data);
//    }
//}