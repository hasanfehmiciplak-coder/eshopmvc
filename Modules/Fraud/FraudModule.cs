using Microsoft.Extensions.DependencyInjection;
using EShopMVC.Modules.Fraud.EventHandlers;
using EShopMVC.Modules.Orders.Events;
using EShopMVC.Shared.EventBus;

namespace EShopMVC.Modules.Fraud
{
    public static class FraudModule
    {
        public static IServiceCollection AddFraudModule(this IServiceCollection services)
        {
            services.AddScoped<IEventHandler<OrderPaidEvent>, OrderPaidFraudCheckHandler>();

            return services;
        }
    }
}