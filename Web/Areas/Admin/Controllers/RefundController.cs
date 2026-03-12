using EShopMVC.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using EShopMVC.Repositories.Refunds;
using EShopMVC.Models;
using EShopMVC.Services.Refunds;
using EShopMVC.Infrastructure.Jobs;
using EShopMVC.Infrastructure.Data;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RefundController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IRefundService _refundService;
        private readonly IRefundRepository _refundRepository;

        public RefundController(
            AppDbContext context,
            IRefundService refundService,
            IRefundRepository refundRepository)
        {
            _context = context;
            _refundService = refundService;
            _refundRepository = refundRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Partial([FromBody] RefundRequestVM model)
        {
            // 1️⃣ BASIC VALIDATION
            if (!ModelState.IsValid)
                return BadRequest("Geçersiz istek");

            // 2️⃣ ORDER + ITEMS
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == model.OrderId);

            if (order == null)
                return NotFound("Sipariş bulunamadı");

            // 3️⃣ ORDER ITEM
            var item = order.OrderItems
                .FirstOrDefault(x => x.Id == model.OrderItemId);

            if (item == null)
                return BadRequest("Sipariş ürünü bulunamadı");

            // 4️⃣ QUANTITY CHECK
            var refundableQty = item.Quantity - item.RefundedQuantity;

            if (model.Quantity <= 0 || model.Quantity > refundableQty)
                return BadRequest("Geçersiz iade miktarı");

            // 5️⃣ PAID CHECK
            if (!order.IsPaid)
                return BadRequest("Ödenmemiş sipariş iade edilemez");

            // FRAUD KONTROLÜ
            if (order.HasActiveHighFraud && !order.RefundOverrideEnabled)
            {
                return BadRequest("Bu siparişte yüksek seviye fraud var. Override gerekli.");
            }

            // 6️⃣ SERVICE CALL (PARA İŞİ)
            var refund = new Refund
            {
                Status = RefundStatus.Pending,
                RetryCount = 0,
                NextRetryAt = null
            };

            _context.Refunds.Add(refund);
            await _context.SaveChangesAsync();

            await _refundService.CreateRefundAsync(refund.Id);

            return Ok(new
            {
                success = refund.Status == RefundStatus.Success
            });
        }

        [HttpPost]
        public async Task<IActionResult> Full(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound("Sipariş bulunamadı");

            if (!order.IsPaid)
                return BadRequest("Ödenmemiş sipariş iade edilemez");

            // 🔒 Sipariş seviyesinde FRAUD kontrolü (tek sefer)
            if (order.HasActiveHighFraud && !order.RefundOverrideEnabled)
            {
                return BadRequest("Bu siparişte yüksek seviye fraud var. Override gerekli.");
            }

            bool anyRefundCreated = false;

            foreach (var item in order.OrderItems)
            {
                var refundableQty = item.Quantity - item.RefundedQuantity;

                if (refundableQty <= 0)
                    continue;

                // 1️⃣ Refund kaydı oluştur
                var refund = new Refund
                {
                    Status = RefundStatus.Pending,
                    RetryCount = 0,
                    NextRetryAt = null
                };

                _context.Refunds.Add(refund);
                await _context.SaveChangesAsync(); // Refund.Id almak için gerekli

                // 2️⃣ Refund işleme başlat (Gateway + Retry + Hangfire)
                await _refundService.CreateRefundAsync(refund.Id);

                anyRefundCreated = true;
            }

            // Hiç iade oluşmadıysa sipariş statüsünü değiştirme
            if (anyRefundCreated)
            {
                order.Status = OrderStatus.Refunded;
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Retry(Guid refundId)
        {
            var refund = await _context.Refunds
                .FirstOrDefaultAsync(r => r.Id == refundId);

            if (refund == null)
                return NotFound("Refund bulunamadı");

            if (refund.Status != RefundStatus.Failed &&
                refund.Status != RefundStatus.PermanentFailed)
            {
                return BadRequest("Bu refund retry edilemez");
            }

            // Retry state reset (kontrollü)
            refund.Status = RefundStatus.Failed;
            refund.NextRetryAt = null;

            await _context.SaveChangesAsync();

            // Retry sürecini tekrar başlat
            await _refundService.RetryRefundAsync(refund.Id);

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> RetryNow(Guid id)
        {
            var refund = await _refundRepository.GetAsync(id);

            if (refund == null)
                return NotFound();

            if (refund.Status == RefundStatus.PermanentFailed)
                return BadRequest("Permanent failed refund retry edilemez.");

            // 🔴 9.6 BURASI
            // Bekleyen scheduled retry varsa etkisizleştir
            refund.NextRetryAt = null;
            await _refundRepository.UpdateAsync(refund);

            // 🔥 Manuel retry – hemen çalışır
            BackgroundJob.Enqueue<RefundRetryJob>(
                job => job.ExecuteAsync(refund.Id));

            TempData["Success"] = "Refund retry job kuyruğa alındı.";

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> Approve(int refundId)
        {
            //await _refundService.ApproveRefundAsync(
            //    refundId,
            //    User.Identity?.Name
            //);

            TempData["Success"] = "İade onaylandı ve işlendi.";
            return RedirectToAction("Index");
        }
    }
}