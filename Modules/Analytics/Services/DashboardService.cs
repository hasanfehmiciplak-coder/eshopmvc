using EShopMVC.Areas.Admin.ViewModels.Dashboard;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models.Fraud;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EShopMVC.Modules.Analytics.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        private const string DashboardCacheKey = "admin_dashboard";

        public DashboardService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<AdminDashboardViewModel> GetDashboard()
        {
            if (_cache.TryGetValue(DashboardCacheKey, out AdminDashboardViewModel vm))
                return vm;

            var today = DateTime.Today;

            // Parallel queries
            var activeFraudTask = _context.FraudFlags
                .CountAsync(f => !f.IsResolved);

            var highFraudTask = _context.FraudFlags
                .CountAsync(f => !f.IsResolved && f.Severity == FraudSeverity.High);

            var todayRefundTask = _context.Refunds
                .CountAsync(r => r.CreatedAt >= today);

            var refundAmountTask = _context.Refunds
                .SumAsync(r => (decimal?)r.Amount);

            var failedPaymentsTask = _context.PaymentLogs
                .CountAsync(p => p.Status != "SUCCESS");

            var cacheKey = $"dashboard:{DateTime.Today:yyyyMMdd}";

            var fraudTask = _context.FraudFlags
                .AsNoTracking()
                .CountAsync(f => !f.IsResolved);

            var fraudTrend = await _context.FraudFlags
                .AsNoTracking()
                .Where(f => f.CreatedAt >= DateTime.Today.AddDays(-30))
                .GroupBy(f => new
                {
                    f.CreatedAt.Year,
                    f.CreatedAt.Month,
                    f.CreatedAt.Day
                })
                .Select(g => new
                {
                    Day = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var refundTask = _context.Refunds
                .AsNoTracking()
                .CountAsync();

            await Task.WhenAll(fraudTask, refundTask);

            await Task.WhenAll(
                activeFraudTask,
                highFraudTask,
                todayRefundTask,
                refundAmountTask,
                failedPaymentsTask
            );

            await _context.FraudFlags
                .AsNoTracking()
                .Where(f => !f.IsResolved)
                .CountAsync();

            vm = new AdminDashboardViewModel
            {
                ActiveFraudCount = activeFraudTask.Result,
                HighFraudCount = highFraudTask.Result,
                TodayRefundCount = todayRefundTask.Result,
                TotalRefundAmount = refundAmountTask.Result ?? 0,
                FailedPaymentCount = failedPaymentsTask.Result
            };

            _cache.Set(
                DashboardCacheKey,
                vm,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            return vm;
        }
    }
}