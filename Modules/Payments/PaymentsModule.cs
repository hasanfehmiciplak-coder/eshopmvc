using EShopMVC.Modules.Payments.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EShopMVC.Modules.Payments
{
    public static class PaymentsModule
    {
        public static IServiceCollection AddPaymentsModule(this IServiceCollection services)
        {
            services.AddScoped<IyzicoService>();
            services.AddScoped<IyzicoRefundService>();

            return services;
        }
    }
}