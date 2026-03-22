using EShopMVC.Areas.Admin.ViewModels;
using EShopMVC.Areas.Admin.ViewModels.Orders;
using EShopMVC.Models.TimeLine;
using EShopMVC.Services;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.Server;
using EShopMVC.Infrastructure.Data;

namespace EShopMVC.Infrastructure.Jobs
{
    public class OrderMailJob
    {
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;

        public OrderMailJob(
            IEmailService emailService,
            AppDbContext context)
        {
            _emailService = emailService;
            _context = context;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task SendOrderSuccessMail(int orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
                throw new Exception("Order not found");

            var userEmail = await _context.Users
                .Where(u => u.Id == order.UserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            var mailModel = new OrderSuccessMailVM
            {
                OrderId = order.Id,
                UserEmail = userEmail,
                TotalPrice = order.TotalPrice,
                OrderDate = order.OrderDate
            };

            await _emailService.SendOrderSuccessMailAsync(mailModel);

            _context.OrderTimelines.Add(new OrderTimeline
            {
                OrderId = order.Id,
                EventType = TimelineEventType.Info,
                Description = "Sipariş emaili gönderildi",
                Details = "Hangfire job başarıyla tamamlandı",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "SYSTEM"
            });

            await _context.SaveChangesAsync();
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task SendPaymentFailedMail(int orderId, string? errorMessage)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
                throw new Exception("Order not found");

            var userEmail = await _context.Users
                .Where(u => u.Id == order.UserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            var vm = new PaymentFailedMailVM
            {
                OrderId = order.Id,
                UserEmail = userEmail,
                ErrorMessage = errorMessage
            };

            await _emailService.SendPaymentFailedMailAsync(vm);

            _context.OrderTimelines.Add(new OrderTimeline
            {
                OrderId = order.Id,
                EventType = TimelineEventType.Warning,
                Description = "Ödeme başarısız maili gönderildi",
                Details = errorMessage,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "SYSTEM"
            });

            await _context.SaveChangesAsync();
        }
    }
}