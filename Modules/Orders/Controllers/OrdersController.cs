using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Models.TimeLine;
using EShopMVC.Modules.Fraud.Services;
using EShopMVC.Modules.Orders.Models;
using EShopMVC.Modules.Payments.Models;
using EShopMVC.Modules.Payments.Public;
using EShopMVC.Services.Orders;
using EShopMVC.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using System.Text.Json;
using IdentityEmailSender = Microsoft.AspNetCore.Identity.UI.Services.IEmailSender;

namespace EShopMVC.Modules.Orders.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IyzicoRestService _iyzico;
        private readonly ICartService _cartService;
        private readonly ILogger<OrdersController> _logger;
        private readonly IdentityEmailSender _emailSender;
        private readonly OrderTimelineService _timelineService;
        private readonly FraudScoreService _fraudScoreService;
        private readonly IPaymentGateway _paymentGateway;

        private readonly OrderTimelineBuilder _orderTimelineBuilder =
            new OrderTimelineBuilder();

        public OrdersController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IyzicoRestService iyzico,
            ICartService cartService,
            ILogger<OrdersController> logger,
            IEmailSender emailSender,
            OrderTimelineService timelineService,
            FraudScoreService fraudScoreService,
            IPaymentGateway paymentGateway,
            OrderTimelineBuilder _orderTimelineBuilder
            )
        {
            _context = context;
            _userManager = userManager;
            _iyzico = iyzico;
            _cartService = cartService;
            _logger = logger;
            _emailSender = emailSender;
            _timelineService = timelineService;
            _fraudScoreService = fraudScoreService;
            _paymentGateway = paymentGateway;
            _orderTimelineBuilder = _orderTimelineBuilder;
        }

        [HttpPost]
        [Authorize]
        public IActionResult Checkout(CheckoutVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Geçersiz form.";
                return RedirectToAction("Checkout");
            }

            TempData["Address"] = vm.Address;
            return RedirectToAction("Payment");
        }

        [Authorize]
        public async Task<IActionResult> Payment(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.UserId == user.Id);

            if (order == null)
                return NotFound();

            if (order.IsPaid)
                return RedirectToAction("Success");

            if (order.Status == OrderStatus.PaymentFailed)
            {
                TempData["Error"] = "Önceki ödeme başarısız oldu. Lütfen tekrar deneyin.";
            }

            ViewBag.Order = order;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PaymentCallback(string token)
        {
            var response = await _iyzico.RetrieveCheckoutFormAsync(token);
            using var doc = JsonDocument.Parse(response);

            var status = doc.RootElement
                .GetProperty("paymentStatus")
                .GetString();

            var conversationId = doc.RootElement
                .TryGetProperty("conversationId", out var cid)
                    ? cid.GetString()
                    : null;

            int? orderId = int.TryParse(conversationId, out var oid)
                ? oid
                : null;

            var transactionId = doc.RootElement
                .GetProperty("itemTransactions")[0]
                .GetProperty("paymentTransactionId")
                .GetString();

            // 🔁 ÇİFT CALLBACK KORUMASI
            if (_context.PaymentLogs.Any(p =>
                p.PaymentTransactionId == transactionId))
            {
                return Ok();
            }

            // 🔎 ORDER'I BUL
            Order order = null;

            if (orderId.HasValue)
            {
                order = await _context.Orders.FindAsync(orderId.Value);
            }

            if (order == null)
            {
                return BadRequest("Order bulunamadı");
            }

            // 🧾 PAYMENT LOG
            var log = new PaymentLog
            {
                OrderId = order.Id,
                Provider = "Iyzico",
                PaymentStatus = status,
                PaidAmount = order.TotalPrice,
                PaymentTransactionId = transactionId,
                ConversationId = conversationId,
                RawResponse = response,
                CreatedAt = DateTime.Now
            };

            _context.PaymentLogs.Add(log);

            // 📦 ORDER GÜNCELLE
            if (status == "SUCCESS")
            {
                order.IsPaid = true;
                order.Status = OrderStatus.Completed;
                order.PaymentTransactionId = transactionId;
            }
            else
            {
                order.Status = OrderStatus.PaymentFailed;
            }

            await _context.SaveChangesAsync();

            TempData["PaymentLogId"] = log.Id;

            return RedirectToAction(
                "PaymentResult",
                new { success = status == "SUCCESS" }
            );
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PaymentResult(bool success)
        {
            if (!success)
            {
                TempData["PaymentError"] = "Ödeme başarısız. Lütfen tekrar deneyin.";
                TempData.Keep("Address");
                return RedirectToAction("Payment");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                TempData["PaymentError"] = "Kullanıcı bilgileri alınamadı.";
                return RedirectToAction("Payment");
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var paymentLogId = TempData["PaymentLogId"] != null
    ? Convert.ToInt32(TempData["PaymentLogId"])
    : (int?)null;

                var order = new Order
                {
                    UserId = user.Id,
                    CustomerEmail = user.Email,                 // ✅ ZORUNLU
                    CustomerName = user.UserName,
                    FullName = user.FullName,// veya FullName varsa
                    Address = TempData["Address"]?.ToString(),
                    TotalPrice = cart.CartItems.Sum(
                        x => x.Product.Price * x.Quantity
                    ),
                    OrderDate = DateTime.UtcNow                  // varsa önerilir
                };
                _context.Orders.Add(order);
                _context.SaveChanges();

                await _timelineService.AddAsync(
                    order.Id,
                    TimelineEventType.OrderCreated,
                    "Sipariş oluşturuldu.",
                    user.Email // veya User.Identity?.Name
                );

                bool paymentSuccess = false;

                // 🔗 PaymentLog → Order bağla
                if (paymentLogId.HasValue)
                {
                    var log = _context.PaymentLogs.Find(paymentLogId.Value);
                    if (log != null)
                    {
                        log.OrderId = order.Id;
                        _context.PaymentLogs.Update(log);

                        // 🔑 SUCCESS kontrolü
                        paymentSuccess = log.PaymentStatus == "SUCCESS";
                    }
                }

                // 🔔 SADECE BAŞARILIYSA timeline ekle
                if (paymentSuccess)
                {
                    await _timelineService.AddAsync(
                        order.Id,
                        TimelineEventType.PaymentReceived,
                        $"Ödeme alındı ({order.TotalPrice} ₺)",
                        "SYSTEM"
                    );
                }

                foreach (var item in cart.CartItems)
                {
                    if (item.Product.Stock < item.Quantity)
                        throw new InvalidOperationException(
                            $"{item.Product.Name} için yeterli stok yok.");

                    item.Product.Stock -= item.Quantity;
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Price = item.Product.Price,
                        Quantity = item.Quantity
                    });
                }

                _context.CartItems.RemoveRange(cart.CartItems);
                _context.SaveChanges();

                transaction.Commit();

                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(
                    ex,
                    "Order create failed | UserId:{UserId}",
                    user.Id
                );

                TempData["Error"] = "Sipariş sırasında hata oluştu.";
                return RedirectToAction("Index", "Cart");
            }
        }

        public IActionResult Success()
        {
            return View();
        }

        // 📦 Siparişlerim
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // 🔍 Sipariş Detayı
        public async Task<IActionResult> Details(int id, TimelineEventType? filter)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
                return NotFound();

            var riskScore = await _fraudScoreService.CalculateScore(order.Id);
            ViewBag.RiskScore = riskScore;

            // 📜 TIMELINE
            var timelines = await _context.OrderTimelines
                .Where(t => t.OrderId == order.Id)
                .ToListAsync();

            var frauds = await _context.FraudFlags
                .Where(f => f.OrderId == order.Id)
                .ToListAsync();

            var timeline = _orderTimelineBuilder.Build(
                order,
                timelines,
                frauds
            );

            ViewBag.Timeline = timeline;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestCancel(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.UserId == user.Id);

            if (order == null)
                return NotFound();

            if (order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = "Bu sipariş artık iptal edilemez.";
                return RedirectToAction("Details", new { id = orderId });
            }

            order.CancelRequested = true;
            order.CancelApproved = false;
            order.CancelRejected = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "İptal talebiniz alındı.";

            return RedirectToAction("Details", new { id = orderId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Review(CheckoutVM vm)
        {
            if (!ModelState.IsValid)

                return RedirectToAction("Checkout");

            var user = await _userManager.GetUserAsync(User);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == vm.AddressId && a.UserId == user.Id);

            if (address == null)
            {
                TempData["Error"] = "Adres bulunamadı.";
                return RedirectToAction("Checkout");
            }

            var reviewVm = new CheckoutReviewVM
            {
                AddressId = address.Id,
                AddressText = $"{address.Title} - {address.City} / {address.District}",

                Items = cart.CartItems.Select(ci => new CheckoutItemVM
                {
                    ProductName = ci.Product.Name,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity
                }).ToList()
            };

            return View(reviewVm);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Complete(int AddressId)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Geçersiz form.";
                return RedirectToAction("Checkout");
            }

            var user = await _userManager.GetUserAsync(User);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Sepetiniz boş.";
                return RedirectToAction("Index", "Cart");
            }

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == AddressId && a.UserId == user.Id);

            if (address == null)
            {
                TempData["Error"] = "Adres bulunamadı.";
                return RedirectToAction("Checkout");
            }

            Order order = null; // 🔑 SCOPE FIX

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 🔒 STOK KONTROLÜ
                foreach (var item in cart.CartItems)
                {
                    if (item.Product.Stock < item.Quantity)
                        throw new InvalidOperationException(
                            $"{item.Product.Name} için yeterli stok yok.");
                }

                order = new EShopMVC.Modules.Orders.Models.Order
                {
                    UserId = user.Id,
                    Address = $"{address.Title} - {address.City} / {address.District}",
                    TotalPrice = cart.CartItems.Sum(
                        x => x.Product.Price * x.Quantity),
                    Status = OrderStatus.Pending,
                    OrderDate = DateTime.Now
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // OrderId oluşsun

                foreach (var item in cart.CartItems)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Price = item.Product.Price,
                        Quantity = item.Quantity
                    });

                    item.Product.Stock -= item.Quantity;
                }

                _context.CartItems.RemoveRange(cart.CartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Siparişiniz başarıyla oluşturuldu.";

                //return RedirectToAction("Success");

                return RedirectToAction(
                    "Pay",
                    "Payment",
                    new { orderId = order.Id }
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Order create failed | UserId:{UserId}",
                    user.Id
                );

                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        [Authorize]
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
                TempData["Error"] = "Bu sipariş için tekrar ödeme yapılamaz.";
                return RedirectToAction("Index", "Orders");
            }

            // 🔒 Retry limiti
            var attemptCount = _context.PaymentLogs
                .Count(p => p.OrderId == order.Id);

            if (attemptCount >= 5)
            {
                TempData["Error"] = "Çok fazla deneme yapıldı.";
                return RedirectToAction("Index", "Orders");
            }

            // 🔁 Aynı Order için ödeme yeniden başlar
            return RedirectToAction("Payment", new { orderId = order.Id });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var cartItems = await _cartService.GetItemsAsync();
            if (!cartItems.Any())
                return BadRequest("Sepet boş");

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new EShopMVC.Modules.Orders.Models.Order
                {
                    UserId = userId,
                    Status = OrderStatus.Pending
                };

                foreach (var item in cartItems)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product == null)
                        throw new Exception("Ürün bulunamadı");

                    // 🔒 STOK KONTROLÜ
                    if (product.Stock < item.Quantity)
                        throw new Exception(
                            $"{product.Name} için yeterli stok yok"
                        );

                    // 🔻 STOK DÜŞ
                    product.Stock -= item.Quantity;

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });

                    order.TotalPrice +=
                        product.Price * item.Quantity;
                }

                _context.Orders.Add(order);

                //            await _emailSender.SendAsync(
                //    Email,
                //    "Siparişiniz alındı",
                //    $"<b>Sipariş No:</b> {order.Id}<br/>Toplam: {order.TotalPrice} ₺"
                //);

                // 🧹 SEPETİ TEMİZLE
                foreach (var item in cartItems)
                    await _cartService.RemoveAsync(item.ProductId);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return RedirectToAction(
                    "Details",
                    new { id = order.Id }
                );
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();

                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }
    }
}