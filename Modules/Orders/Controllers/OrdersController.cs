using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Modules.Orders.Application;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EShopMVC.Modules.Orders.Application.Services;

namespace EShopMVC.Modules.Orders.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CheckoutService _checkoutService;
        private readonly RefundService _refundService;

        public OrdersController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            CheckoutService checkoutService,
            RefundService refundService)
        {
            _context = context;
            _userManager = userManager;
            _checkoutService = checkoutService;
            _refundService = refundService;
        }

        // 📦 Sipariş oluştur (Checkout)
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            try
            {
                var order = await _checkoutService.CreateOrderFromCart(userId);

                return RedirectToAction("Details", new { id = order.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        // 💳 Ödeme ekranı
        public async Task<IActionResult> Payment(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.UserId == user.Id);

            if (order == null)
                return NotFound();

            if (order.Status == OrderStatus.PaymentFailed)
            {
                TempData["Error"] = "Ödeme başarısız. Tekrar deneyin.";
            }

            ViewBag.Order = order;
            return View();
        }

        // 📦 Siparişlerim
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // 🔍 Detay
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.UserId == user.Id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // 🔁 Tekrar ödeme
        public async Task<IActionResult> RetryPayment(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.UserId == user.Id &&
                    o.Status == OrderStatus.PaymentFailed);

            if (order == null)
            {
                TempData["Error"] = "Tekrar ödeme yapılamaz.";
                return RedirectToAction("MyOrders");
            }

            return RedirectToAction("Payment", new { orderId = order.Id });
        }

        [HttpPost]
        public async Task<IActionResult> RequestRefund(int orderId, string reason)
        {
            await _refundService.RequestRefund(orderId, reason);

            TempData["Success"] = "Refund request created";

            return RedirectToAction("Details", new { id = orderId });
        }
    }
}