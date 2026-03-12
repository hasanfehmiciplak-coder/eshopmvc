//using EShopMVC.Modules.Orders.Interfaces;
//using EShopMVC.Modules.Orders.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EShopMVC.Modules.Orders
{
    public static class OrdersModule
    {
        public static IServiceCollection AddOrdersModule(this IServiceCollection services)
        {
            services.AddScoped<ICartService, CartService>();

            return services;
        }
    }
}