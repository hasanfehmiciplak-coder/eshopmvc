using EShopMVC.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using EShopMVC.Repositories.Refunds;
using EShopMVC.Models;
using EShopMVC.Modules.Orders.Application.Services;
using EShopMVC.Infrastructure.Jobs;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Orders.Domain.Enums;
using Refund = EShopMVC.Modules.Orders.Domain.Entities.Refund;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RefundController : Controller
    {
        private readonly AppDbContext _context;
        private readonly RefundService _refundService;
        private readonly IRefundRepository _refundRepository;

        public RefundController(
            AppDbContext context,
            RefundService refundService,
            IRefundRepository refundRepository)
        {
            _context = context;
            _refundService = refundService;
            _refundRepository = refundRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Partial([FromBody] RefundRequestVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Geçersiz istek");

            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.FraudFlags)
                .FirstOrDefaultAsync(o => o.Id == model.OrderId);

            if (order == null)
                return NotFound("Sipariş bulunamadı");

            var item = order.Items.FirstOrDefault(x => x.Id == model.OrderItemId);

            if (item == null)
                return BadRequest("Sipariş ürünü bulunamadı");

            var refundableQty = item.Quantity - item.RefundedQuantity;

            if (model.Quantity <= 0 || model.Quantity > refundableQty)
                return BadRequest("Geçersiz iade miktarı");

            if (!order.IsPaid)
                return BadRequest("Ödenmemiş sipariş iade edilemez");

            if (order.HasActiveHighFraud && !order.RefundOverrideEnabled)
                return BadRequest("Bu siparişte yüksek seviye fraud var. Override gerekli.");

            var refund = new Refund(item.Id, model.Quantity, item.Price * model.Quantity);

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
                .Include(o => o.Items)
                .Include(o => o.FraudFlags)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound("Sipariş bulunamadı");

            if (!order.IsPaid)
                return BadRequest("Ödenmemiş sipariş iade edilemez");

            if (order.HasActiveHighFraud && !order.RefundOverrideEnabled)
            {
                return BadRequest("Bu siparişte yüksek seviye fraud var. Override gerekli.");
            }

            var refunds = new List<Refund>();

            foreach (var item in order.Items)
            {
                var refundableQty = item.Quantity - item.RefundedQuantity;

                if (refundableQty <= 0)
                    continue;

                refunds.Add(new Refund(item.Id, refundableQty, item.Price * refundableQty));
            }

            if (!refunds.Any())
                return Ok(new { success = false });

            _context.Refunds.AddRange(refunds);

            await _context.SaveChangesAsync(); // ✅ TEK DB CALL

            // Refund işlemlerini başlat
            foreach (var refund in refunds)
            {
                BackgroundJob.Enqueue<RefundRetryJob>(
                job => job.ExecuteAsync(refund.Id));
            }

            order.MarkAsRefunded();

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Retry(int refundId)
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
        public async Task<IActionResult> RetryNow(int id)
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
            TempData["Success"] = "İade onaylandı ve işlendi.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Pending()
        {
            var refunds = await _context.Refunds
                //.Include(x => x.OrderItem)
                .Where(x => x.Status == RefundStatus.Pending)
                .ToListAsync();

            return View(refunds);
        }
    }
}