using EShopMVC.Modules.Orders.Events;
using EShopMVC.Modules.Fraud.Services;
using EShopMVC.Shared.EventBus;
using Microsoft.EntityFrameworkCore;
using EShopMVC.Infrastructure.Data;

namespace EShopMVC.Modules.Fraud.EventHandlers
{
    public class OrderPaidFraudCheckHandler : IEventHandler<OrderPaidEvent>
    {
        private readonly FraudEvaluationService _fraudService;
        private readonly AppDbContext _context;

        public OrderPaidFraudCheckHandler(
            FraudEvaluationService fraudService,
            AppDbContext context)
        {
            _fraudService = fraudService;
            _context = context;
        }

        public async Task HandleAsync(OrderPaidEvent domainEvent)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == domainEvent.OrderId);

            if (order == null)
                return;

            await _fraudService.EvaluateOrderAsync(order);
        }
    }
}