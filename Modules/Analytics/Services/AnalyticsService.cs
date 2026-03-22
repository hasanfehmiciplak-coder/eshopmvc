using EShopMVC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Modules.Analytics.Services
{
    public class AnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTodayRevenue()
        {
            var today = DateTime.Today;

            return await _context.Orders
                .Where(o => o.OrderDate >= today && o.IsPaid)
                .SumAsync(o => o.TotalPrice);
        }

        public async Task<int> GetTodayOrderCount()
        {
            var today = DateTime.Today;

            return await _context.Orders
                .CountAsync(o => o.OrderDate >= today);
        }

        public async Task<int> GetFraudAlertCount()
        {
            return await _context.FraudFlags
                .CountAsync(f => !f.IsResolved);
        }

        public async Task<int> GetRefundCount()
        {
            return await _context.Refunds.CountAsync();
        }
    }
}