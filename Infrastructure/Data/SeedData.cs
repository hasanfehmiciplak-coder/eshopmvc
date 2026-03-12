using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Models;
using EShopMVC.Modules.Payments.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Data.Seed
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

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

            //// Rol atama
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                await userManager.AddToRoleAsync(adminUser, "Admin");

            if (!context.Firms.Any())
            {
                context.Firms.Add(new Firm
                {
                    Name = "Hasan Ticaret",
                    //Address = "İstanbul, Türkiye",
                    //Phone = "0555 555 55 55",
                    //Email = "info@hasanticaret.com"
                });
                await context.SaveChangesAsync();
            }

            if (!context.Orders.Any())
            {
                var order = new Order
                {
                    UserId = adminUser.Id,
                    OrderDate = DateTime.UtcNow,
                    TotalPrice = 1500
                };

                context.Orders.Add(order);
                await context.SaveChangesAsync();

                var payment = new PaymentLog
                {
                    OrderId = order.Id,
                    Status = "Paid",
                    CreatedAt = DateTime.UtcNow
                };

                context.PaymentLogs.Add(payment);

                var refund = new RefundLog
                {
                    OrderId = order.Id,
                    Reason = "Customer requested refund",
                    CreatedAt = DateTime.UtcNow.AddMinutes(2)
                };

                context.RefundLogs.Add(refund);

                var fraud = new FraudFlag
                {
                    OrderId = order.Id,
                    Reason = FraudReason.RefundTooFast,
                    CreatedAt = DateTime.UtcNow.AddMinutes(3)
                };
                context.FraudFlags.Add(fraud);

                await context.SaveChangesAsync();
            }
        }

        //public static void Initialize(AppDbContext context)
        //{
        //    context.Database.Migrate();

        //    if (context.Orders.Any())
        //        return;

        //    var order = new Order
        //    {
        //        UserId = "demo-user",
        //        OrderDate = DateTime.UtcNow,
        //        TotalPrice = 1200
        //    };

        //    context.Orders.Add(order);
        //    context.SaveChanges();

        //    var payment = new PaymentLog
        //    {
        //        OrderId = order.Id,
        //        Status = "Paid",
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    context.PaymentLogs.Add(payment);

        //    var refund = new RefundLog
        //    {
        //        OrderId = order.Id,
        //        Reason = "Customer request",
        //        CreatedAt = DateTime.UtcNow.AddMinutes(5)
        //    };

        //    context.RefundLogs.Add(refund);

        //    var fraud = new FraudFlag
        //    {
        //        OrderId = order.Id,
        //        Reason = "Refund shortly after payment",
        //        CreatedAt = DateTime.UtcNow.AddMinutes(6)
        //    };

        //    context.FraudFlags.Add(fraud);

        //    context.SaveChanges();
        //}
    }
}