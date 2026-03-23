using EShopMVC.Helpers;
using EShopMVC.Hubs;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Infrastructure.Jobs;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Models.TimeLine;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Fraud.Services;
using EShopMVC.Modules.Orders.Domain.Enums;
using EShopMVC.Modules.Orders.Domain.Logs;
using EShopMVC.Modules.Payments.Models;
using Hangfire;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;
using IyzicoOptions = EShopMVC.Models.IyzicoOptions;

namespace EShopMVC.Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IOptions<IyzicoOptions> _iyzicoOptions;
        private readonly OrderTimelineService _timelineService;
        private readonly IHubContext<FraudHub> _fraudHub;
        private readonly FraudScoreService _fraudScoreService;
        private readonly UserFraudService _userFraudService;
        private readonly FraudPatternService _fraudPatternService;
        private readonly FraudPredictionService _fraudPredictionService;

        public PaymentController(
            AppDbContext context,
            IOptions<IyzicoOptions> iyzicoOptions,
            OrderTimelineService timelineService,
            IHubContext<FraudHub> fraudHub,
            FraudScoreService fraudScoreService,
            UserFraudService userFraudService,
            FraudPatternService _fraudPatternService,
            FraudPredictionService fraudPredictionService
            )
        {
            _context = context;
            _iyzicoOptions = iyzicoOptions;
            _timelineService = timelineService;
            _fraudHub = fraudHub;
            _fraudScoreService = fraudScoreService;
            _userFraudService = userFraudService;
            _fraudPatternService = _fraudPatternService;
            _fraudPredictionService = fraudPredictionService;
        }

        [HttpGet]
        public async Task<IActionResult> StartPayment(int orderId)
        {
            var order = _context.Orders
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == orderId);

            if (order == null)
                return NotFound();

            var options = new Iyzipay.Options
            {
                ApiKey = _iyzicoOptions.Value.ApiKey.Trim(),
                SecretKey = _iyzicoOptions.Value.SecretKey.Trim(),
                BaseUrl = "https://sandbox-api.iyzipay.com"
            };

            var request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = orderId.ToString(),   // 🔥 BURASI
                Price = "1.00",
                PaidPrice = "1.00",
                Currency = Currency.TRY.ToString(),
                BasketId = orderId.ToString(),         // 🔥 BU DA
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = "https://localhost:7099/Payment/Callback"
            };

            request.Buyer = new Buyer
            {
                Id = "BY789",
                Name = "Test",
                Surname = "User",
                Email = "test@test.com",
                IdentityNumber = "11111111111",
                RegistrationAddress = "Test Address",
                Ip = "85.34.78.112",
                City = "Istanbul",
                Country = "Turkey"
            };

            request.ShippingAddress = new Address
            {
                ContactName = "Test User",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Test Shipping Address",
                ZipCode = "34000"
            };

            request.BillingAddress = new Address
            {
                ContactName = "Test User",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Test Billing Address",
                ZipCode = "34000"
            };

            request.BasketItems = new List<BasketItem>
                {
                    new BasketItem
                    {
                        Id = "BI101",
                        Name = "Test Item",
                        Category1 = "Test",
                        ItemType = BasketItemType.PHYSICAL.ToString(),
                        Price = "1.00"
                    }
                };

            await _timelineService.AddAsync(
                order.Id,
                TimelineEventType.PaymentInitiated,
                "Ödeme başlatıldı",
                User.Identity?.Name ?? "SYSTEM"
            );

            var checkoutFormInitialize = CheckoutFormInitialize.Create(request, options);

            if (checkoutFormInitialize.Status != "success")
            {
                return Content(
                    $"Status: {checkoutFormInitialize.Status}\n" +
                    $"ErrorCode: {checkoutFormInitialize.ErrorCode}\n" +
                    $"ErrorMessage: {checkoutFormInitialize.ErrorMessage}"
                );
            }

            ViewBag.CheckoutForm = checkoutFormInitialize.CheckoutFormContent;
            return View("Payment");
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Callback()
        {
            // 1️⃣ FORM KONTROLÜ
            if (!Request.HasFormContentType)
                return BadRequest("Form content yok");

            var token = Request.Form["token"].ToString();

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token yok");

            // 2️⃣ IYZICO OPTIONS
            var iyzipayOptions = new Iyzipay.Options
            {
                ApiKey = _iyzicoOptions.Value.ApiKey,
                SecretKey = _iyzicoOptions.Value.SecretKey,
                BaseUrl = _iyzicoOptions.Value.BaseUrl
            };

            // 3️⃣ ÖDEMEYİ IYZICO'DAN ÇEK
            var response = CheckoutForm.Retrieve(
                new RetrieveCheckoutFormRequest
                {
                    Token = token
                },
                iyzipayOptions
            );
            var rawResponse = JsonConvert.SerializeObject(response);

            var basketId = response.BasketId;

            if (!int.TryParse(basketId, out int orderId))
                return BadRequest("BasketId parse edilemedi");

            // 5️⃣ TRANSACTION
            var transaction = response.PaymentItems?.FirstOrDefault();
            var transactionId = transaction?.PaymentTransactionId;

            if (string.IsNullOrEmpty(transactionId))
                return BadRequest("TransactionId yok");

            // 🔁 DOUBLE CALLBACK KORUMASI
            if (_context.PaymentLogs.Any(x => x.PaymentTransactionId == transactionId))
                return Ok();

            // 6️⃣ ORDER (logdan ÖNCE alıyoruz ki PaidAmount vs doğru olsun)
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            // 7️⃣ PAYMENT LOG (SADECE 1 TANE)
            var log = new PaymentLog(
                order.Id,
                order.TotalPrice,
                "Iyzico",
                response.PaymentStatus ?? "FAILED"
            );
            _context.PaymentLogs.Add(log);

            // 8️⃣ ORDER STATUS
            if (response.Status == "SUCCESS")
            {
                order.IsPaid = true;
                order.SetStatus(OrderStatus.Paid);

                BackgroundJob.Enqueue<OrderMailJob>(
                    job => job.SendOrderSuccessMail(order.Id)
                );
            }
            else
            {
                order.SetStatus(OrderStatus.PaymentFailed);

                BackgroundJob.Enqueue<OrderMailJob>(
                    job => job.SendPaymentFailedMail(
                        order.Id,
                        response.ErrorMessage
                    )
                );
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            var probability =
            await _fraudPredictionService.PredictOrderFraud(order.Id);

            if (probability >= 0.80)
            {
                order.SetStatus(OrderStatus.Blocked);

                var fraud = FraudFlag.CreateRefundTooFast(order.Id);

                _context.FraudFlags.Add(fraud);

                await _timelineService.AddAsync(
                    order.Id,
                    TimelineEventType.Fraud,
                    "🚫 Real-time fraud detection ile sipariş bloklandı",
                    "SYSTEM"
                );

                await _context.SaveChangesAsync();
            }

            await _userFraudService.UpdateUserScore(order.UserId);
            await _fraudScoreService.CheckVelocityRule(order.Id);

            // 🔔 TIMELINE (DOĞRU VE SADE)
            if (response.Status == "SUCCESS")
            {
                await _timelineService.AddAsync(
                    order.Id,
                    TimelineEventType.PaymentReceived,
                    "Ödeme başarıyla alındı",
                    "SYSTEM"
                );

                await _timelineService.AddAsync(
                    orderId,
                    TimelineEventType.Fraud,
                    "🚨 Aynı IP'den çoklu ödeme denemesi",
                    "SYSTEM");

                await _timelineService.AddAsync(
                    orderId,
                    TimelineEventType.Fraud,
                    "🚨 Velocity fraud tespit edildi (çok hızlı ödeme denemeleri)",
                    "SYSTEM");

                TempData["ShowPaymentSuccess"] = true;
                TempData["NewTimelineEvent"] = TimelineEventType.PaymentReceived.ToString();
                TempData["ToastSuccess"] =
                    $"Ödeme alındı 🎉 <a href='/Order/Detail/{order.Id}'>Sipariş #{order.Id}</a>";
            }
            else
            {
                var reason = IyzicoErrorMapper.Map(
                    response.ErrorCode,
                    response.ErrorMessage
                );

                await _timelineService.AddAsync(
                    order.Id,
                    TimelineEventType.PaymentFailed,
                    reason,
                    "SYSTEM"
                );

                // 🔎 Ödeme başarısız sayısını kontrol et
                var failCount = await _context.PaymentLogs
                    .Where(x => x.OrderId == order.Id && x.Status != "SUCCESS")
                    .CountAsync();

                var existingFlag = await _context.FraudFlags
                    .AnyAsync(x =>
                        x.OrderId == order.Id &&
                        x.RuleCode == "PAYMENT_RETRY_LIMIT");

                if (!existingFlag && failCount >= 5)
                {
                    //_context.FraudFlags.Add(new FraudFlag
                    //{
                    //    OrderId = order.Id,
                    //    RuleCode = "PAYMENT_RETRY_LIMIT",
                    //    Description = "Bu sipariş için çok fazla ödeme denemesi yapıldı.",
                    //    CreatedAt = DateTime.UtcNow
                    //});

                    await _context.SaveChangesAsync();

                    await _fraudPatternService.DetectIpPattern();

                    await _fraudScoreService.CheckAutoBlock(order.Id);

                    // 🔔 Timeline Fraud Event
                    await _timelineService.AddAsync(
                        order.Id,
                        TimelineEventType.Fraud,
                        "⚠️ Çok fazla ödeme denemesi tespit edildi",
                        "SYSTEM"
                    );

                    await _fraudHub.Clients.All.SendAsync(
                        "FraudDetected",
                        new
                        {
                            orderId = order.Id,
                            rule = "PAYMENT_RETRY_LIMIT",
                            message = "Çok fazla ödeme denemesi"
                        });
                }

                TempData["ToastError"] =
                    "Ödeme alınamadı ❌ Lütfen kart bilgilerinizi kontrol edip tekrar deneyin.";
            }

            // 9️⃣ REDIRECT
            if (User.Identity != null && User.Identity.IsAuthenticated &&
                User.IsInRole("Admin"))
            {
                return RedirectToAction(
                    "Details",
                    "Orders",
                    new { area = "Admin", id = order.Id }
                );
            }

            TempData["ShowPaymentSuccess"] = true;
            TempData["NewTimelineEvent"] = TimelineEventType.PaymentReceived.ToString();

            TempData["ToastSuccess"] =
                $"Ödeme alındı 🎉 <a href='/Order/Detail/{order.Id}'>Sipariş #{order.Id}</a>";

            TempData["Info"] = "Sepetiniz başarıyla temizlendi.";

            return RedirectToAction("Success", new { orderId = order.Id });
        }

        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            return View(order);
        }

        public async Task<IActionResult> RetryPayment(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                 .ThenInclude(oi => oi.ProductName)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            if (order.Status != OrderStatus.PaymentFailed)
                return BadRequest("Bu sipariş tekrar ödenemez.");

            await _timelineService.AddAsync(
                order.Id,
                TimelineEventType.Info,
                "🔁 Retry: Kullanıcı ödemeyi tekrar denedi",
                order.UserId
            );
            // 1️⃣ IYZICO OPTIONS
            var iyzipayOptions = new Iyzipay.Options
            {
                ApiKey = _iyzicoOptions.Value.ApiKey,
                SecretKey = _iyzicoOptions.Value.SecretKey,
                BaseUrl = _iyzicoOptions.Value.BaseUrl
            };

            // 2️⃣ REQUEST
            var request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = order.Id.ToString(),
                Price = order.TotalPrice.ToString("0.00", CultureInfo.InvariantCulture),
                PaidPrice = order.TotalPrice.ToString("0.00", CultureInfo.InvariantCulture),
                Currency = Currency.TRY.ToString(),
                BasketId = order.Id.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = Url.Action(
                    "Callback",
                    "Payment",
                    null,
                    Request.Scheme
                )
            };

            // 3️⃣ BASKET ITEMS
            request.BasketItems = order.Items.Select(item => new BasketItem
            {
                Id = item.Id.ToString(),
                Name = item.ProductName,
                Category1 = "Ürün",
                ItemType = BasketItemType.PHYSICAL.ToString(),
                Price = (item.Price * item.Quantity)
                    .ToString("0.00", CultureInfo.InvariantCulture)
            }).ToList();

            // 4️⃣ CREATE CHECKOUT FORM
            var checkoutForm = CheckoutFormInitialize.Create(
                request,
                iyzipayOptions
            );

            return View("Checkout", checkoutForm);
        }
    }
}