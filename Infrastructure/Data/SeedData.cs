using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Logs;
using EShopMVC.Modules.Orders.Domain.Refunds;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Infrastructure.Data.Seed
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            // Admin rolü yoksa oluştur
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Admin kullanıcı yoksa oluştur
            var adminEmail = "admin@eshop.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (!result.Succeeded)
                    throw new Exception(string.Join(" | ", result.Errors.Select(e => e.Description)));
            }

            // Rol atama
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                await userManager.AddToRoleAsync(adminUser, "Admin");

            if (!context.Orders.Any())
            {
                var order = new Order(adminUser.Id, 1500);

                context.Orders.Add(order);
                await context.SaveChangesAsync();

                var payment = new PaymentLog(
                    order.Id,
                    1500,
                    "CreditCard",
                    "SUCCESS"
                );

                context.PaymentLogs.Add(payment);

                var refund = new Modules.Orders.Domain.Entities.Refund(
                    order.Id,
                    1,
                    1500

                );

                context.Refunds.Add(refund);

                var fraud = new FraudFlag(
                    order.Id,
                    "REFUND_TOO_FAST",
                    FraudSeverity.Medium,
                    "Ödeme sonrası çok kısa sürede iade alındı"
                );

                context.FraudFlags.Add(fraud);

                await context.SaveChangesAsync();
            }
        }
    }
}