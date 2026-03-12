using EShopMVC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Modules.Fraud.Services
{
    public class BehaviorScoreService
    {
        private readonly AppDbContext _context;

        public BehaviorScoreService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CalculateBehaviorScore(int orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
                return 0;

            int score = 0;

            // gece siparişi
            if (order.CreatedDate.Hour < 5)
                score += 10;

            // yüksek fiyatlı sipariş
            if (order.TotalPrice > 5000)
                score += 20;

            return score;
        }
    }
}