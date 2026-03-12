using EShopMVC.Areas.Admin.ViewModels;
using EShopMVC.Areas.Admin.ViewModels.Orders;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Models.Options;
using EShopMVC.Models.TimeLine;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Models;
using EShopMVC.Modules.Payments.Models;
using EShopMVC.Services.Orders;
using EShopMVC.Services.Refunds;
using EShopMVC.Web.ViewModels;
using Iyzipay.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Security.Claims;
using IdentityEmailSender = Microsoft.AspNetCore.Identity.UI.Services.IEmailSender;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IdentityEmailSender _emailSender;
        private readonly int _undoRefundHourLimit;
        private readonly IOrderDetailsService _orderDetailsService;
        private readonly IMemoryCache _cache;
        private readonly OrderTimelineService _timelineService;
        private readonly ICartService _cartService;
        private readonly IRefundService _refundService;
        private readonly OrderTimelineBuilder _orderTimelineBuilder;
        private readonly OrderRiskService _riskService;
        private readonly FraudPredictionService _fraudPredictionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(
            AppDbContext context,
            IEmailSender emailSender,
            IOptions<RefundSettings> refundOptions,
            IOrderDetailsService orderDetailsService,
            OrderTimelineService timelineService,
            IMemoryCache cache,
            ICartService cartService,
            IRefundService refundService,
            OrderTimelineBuilder orderTimelineBuilder,
            OrderRiskService riskService,
            FraudPredictionService fraudPredictionService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _undoRefundHourLimit = refundOptions.Value.UndoHourLimit;
            _orderDetailsService = orderDetailsService;
            _timelineService = timelineService;
            _cache = cache;
            _cartService = cartService;
            _refundService = refundService;
            _orderTimelineBuilder = orderTimelineBuilder;
            _riskService = riskService;
            _fraudPredictionService = fraudPredictionService;
            _userManager = userManager;
        }

        private const int UndoRefundHourLimit = 24; // 🔥 24 saat

        [HttpGet]
        public IActionResult Test()
        {
            return Content("ADMIN ORDER TEST OK");
        }

        // 📦 Sipariş Listesi
        public async Task<IActionResult> Index(string fraud, string risk)
        {
            var fraudSummary = new
            {
                High = await _context.FraudFlags.CountAsync(f => f.Severity == FraudSeverity.High && !f.IsResolved),

                Medium = await _context.FraudFlags.CountAsync(f => f.Severity == FraudSeverity.Medium && !f.IsResolved),

                Active = await _context.FraudFlags.CountAsync(f => !f.IsResolved)
            };

            var resolvedFrauds = await _context.FraudFlags
                .Where(f => f.IsResolved && f.ResolvedAt.HasValue)
                .Select(f => new
                {
                    f.CreatedAt,
                    f.ResolvedAt
                })
                .ToListAsync();

            TimeSpan? avgResolutionTime = null;

            if (resolvedFrauds.Any())
            {
                avgResolutionTime = TimeSpan.FromSeconds(
                    resolvedFrauds.Average(f =>
                        (f.ResolvedAt!.Value - f.CreatedAt).TotalSeconds)
                );
            }

            ViewBag.FraudAvgResolutionTime = avgResolutionTime;

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.PaymentLogs)
                .Include(o => o.FraudFlags)
                .AsQueryable();

            if (fraud == "high")
            {
                query = query.Where(o =>
                    o.FraudFlags.Any(f =>
                        !f.IsResolved &&
                        f.Severity == FraudSeverity.High));
            }
            else if (fraud == "active")
            {
                query = query.Where(o =>
                    o.FraudFlags.Any(f => !f.IsResolved));
            }
            else if (fraud == "medium")
            {
                query = query.Where(o =>
                    o.FraudFlags.Any(f =>
                        !f.IsResolved &&
                        f.Severity == FraudSeverity.Medium));
            }

            if (risk == "high")
            {
                query = query.Where(o =>
                    _context.UserFraudScores.Any(u =>
                        u.UserId == o.UserId &&
                        u.RiskLevel == "High"));
            }

            var orders = await query
                .Select(o => new AdminOrderIndexVM
                {
                    Id = o.Id,
                    UserEmail = o.User.Email,
                    TotalPrice = o.TotalPrice,
                    OrderDate = o.OrderDate,

                    HasHighFraud = o.FraudFlags.Any(f =>
                        f.Severity == FraudSeverity.High && !f.IsResolved),

                    HasMediumFraud = o.FraudFlags.Any(f =>
                        f.Severity == FraudSeverity.Medium && !f.IsResolved),

                    RefundOverrideEnabled = o.RefundOverrideEnabled
                })
                .ToListAsync();

            ViewBag.FraudFilter = fraud;
            ViewBag.RiskFilter = risk;
            ViewBag.FraudSummary = fraudSummary;

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, TimelineEventType? filter)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.PartialRefunds)
                .Include(o => o.FraudFlags)
                .Include(o => o.PaymentLogs) // ← çoğul ise
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var timelines = await _context.OrderTimelines
                .Where(t => t.OrderId == order.Id)
                .ToListAsync();

            var frauds = order.FraudFlags?.ToList() ?? new List<FraudFlag>();

            var fraudProbability =
                await _fraudPredictionService.PredictOrderFraud(order.Id);

            ViewBag.FraudProbability = fraudProbability;

            var timeline = _orderTimelineBuilder.Build(
                order,
                timelines,
                frauds
            );

            if (filter.HasValue)
            {
                timeline = timeline
                    .Where(x => x.EventType == filter.Value)
                    .ToList();
            }

            var resolverUserIds = frauds
                .Where(f => f.ResolvedByUserId != null)
                .Select(f => f.ResolvedByUserId!)
                .Distinct()
                .ToList();

            var resolverEmails = await _context.Users
                .Where(u => resolverUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Email);

            var paymentLogs = order.PaymentLogs ?? new List<PaymentLog>();

            var paymentAttemptCount = paymentLogs.Count;

            var successfulPaymentCount = paymentLogs.Count(x =>
                x.PaymentStatus == "SUCCESS");

            var failedPaymentCount = paymentLogs.Count(x =>
                x.PaymentStatus != "SUCCESS");

            var vm = new OrderDetailViewModel
            {
                Order = order,
                Timeline = timeline,
                PaymentAttemptCount = paymentAttemptCount,
                SuccessfulPaymentCount = successfulPaymentCount,
                FailedPaymentCount = failedPaymentCount,

                FraudFlags = frauds.Select(f => new FraudFlagItemVM
                {
                    Id = f.Id,
                    RuleCode = f.RuleCode,
                    Severity = f.Severity,
                    Description = f.Description,

                    IsResolved = f.IsResolved,
                    ResolvedAt = f.ResolvedAt,
                    ResolutionNote = f.ResolutionNote,
                    // 🔴 BURASI DEĞİŞTİ
                    ResolvedByUserEmail = f.ResolvedByUserId == null
                    ? null
                    : resolverEmails.GetValueOrDefault(f.ResolvedByUserId),

                    CreatedAt = f.CreatedAt
                })
                .ToList()
            };

            var riskScore = await _riskService.CalculateRiskScore(order.Id);

            ViewBag.RiskScore = riskScore;

            return View(vm);
        }

        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var pdf = new InvoiceDocument(order).GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Fatura_{order.Id}.pdf"
            );
        }

        public async Task<IActionResult> CancelRequests()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.CancelRequested && !o.CancelApproved && !o.CancelRejected)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCancel(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            //order.Status = OrderStatus.IptalEdildi;
            order.CancelApproved = true;
            order.CancelRequested = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Sipariş iptal edildi.";

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> RejectCancel(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            order.CancelRejected = true;
            order.CancelRequested = false;

            await _context.SaveChangesAsync();

            TempData["Info"] = "İptal talebi reddedildi.";

            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public IActionResult InvoicePdf(int id)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        // HEADER
                        col.Item().Text("FATURA")
                            .FontSize(20)
                            .Bold();

                        col.Item().Text($"Sipariş No: {order.Id}");
                        col.Item().Text($"Tarih: {order.OrderDate:dd.MM.yyyy}");
                        col.Item().Text($"Müşteri: {order.User.Email}");
                        col.Item().Text($"Adres: {order.Address}");

                        col.Item().LineHorizontal(1);

                        // TABLE HEADER
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Ürün").Bold();
                                header.Cell().Text("Fiyat");
                                header.Cell().Text("Adet");
                                header.Cell().Text("Toplam");
                            });

                            foreach (var item in order.OrderItems)
                            {
                                table.Cell().Text(item.Product.Name);
                                table.Cell().Text($"{item.Price} ₺");
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text($"{item.Price * item.Quantity} ₺");
                            }
                        });

                        col.Item().LineHorizontal(1);

                        // TOTAL
                        col.Item().AlignRight().Text($"GENEL TOPLAM: {order.TotalPrice} ₺")
                            .FontSize(14)
                            .Bold();
                    });
                });
            });

            var pdf = document.GeneratePdf();

            return File(pdf, "application/pdf", $"fatura-{order.Id}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> CreatePartial(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.FraudFlags)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            // 🚨 HIGH FRAUD KİLİDİ
            if (order.FraudFlags.Any(f =>
                    f.Severity == FraudSeverity.High &&
                    !f.IsResolved) &&
                !order.RefundOverrideEnabled)
            {
                TempData["Error"] =
                    "HIGH Fraud tespit edilen siparişlerde iade başlatılamaz.";
                return RedirectToAction("Details", new { id = order.Id });
            }

            // 🟡 MEDIUM FRAUD – ONAY LOG
            if (order.FraudFlags.Any(f =>
                    f.Severity == FraudSeverity.Medium &&
                    !f.IsResolved))
            {
                await _timelineService.AddAsync(
                    order.Id,
                    TimelineEventType.Fraud,
                    "MEDIUM Fraud için iade onayı alındı.",
                    User.Identity?.Name
                );
            }

            var vm = new CreatePartialRefundVM
            {
                OrderId = order.Id,
                Items = order.OrderItems.Select(oi => new RefundItemVM
                {
                    OrderItemId = oi.Id,
                    ProductName = oi.Product.Name,
                    OrderedQuantity = oi.Quantity,
                    AlreadyRefundedQuantity = oi.RefundedQuantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePartial(CreatePartialRefundVM vm)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.PartialRefunds)
                .Include(o => o.FraudFlags)
                .FirstOrDefaultAsync(o => o.Id == vm.OrderId);

            if (order == null)
                return NotFound();

            // 🚨 HIGH FRAUD KİLİDİ
            if (order.FraudFlags.Any(f =>
                    f.Severity == FraudSeverity.High &&
                    !f.IsResolved) &&
                !order.RefundOverrideEnabled)
            {
                TempData["Error"] =
                    "HIGH Fraud tespit edilen siparişlerde iade talebi oluşturulamaz.";
                return RedirectToAction("Details", new { id = order.Id });
            }

            var paidTotal = await _context.PaymentLogs
                .Where(p => p.OrderId == order.Id && p.PaymentStatus == "SUCCESS")
                .SumAsync(p => p.PaidAmount);

            var refundedTotal = order.PartialRefunds
                .Where(r => r.Status == RefundStatus.Completed)
                .Sum(r => r.RefundAmount);

            var requestedRefund = vm.Items
                .Where(i => i.RefundQuantity > 0)
                .Sum(i => i.RefundQuantity * i.UnitPrice);

            if (refundedTotal + requestedRefund > paidTotal)
            {
                TempData["Error"] =
                    $"İade tutarı ödenen toplamı geçemez.";
                return RedirectToAction("CreatePartial", new { orderId = order.Id });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in vm.Items.Where(i => i.RefundQuantity > 0))
                {
                    var orderItem = order.OrderItems.First(x => x.Id == item.OrderItemId);

                    _context.PartialRefunds.Add(new PartialRefund
                    {
                        OrderId = order.Id,
                        OrderItemId = orderItem.Id,
                        Quantity = item.RefundQuantity,
                        RefundAmount = item.RefundQuantity * item.UnitPrice,
                        Reason = item.Reason,
                        Status = RefundStatus.PendingApproval,
                        RequestedByUserId =
                            User.FindFirstValue(ClaimTypes.NameIdentifier)
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    "İade talebi oluşturuldu. Finans onayı bekleniyor.";
                return RedirectToAction("Details", new { id = order.Id });
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "İade talebi oluşturulurken hata oluştu.";
                return RedirectToAction("CreatePartial", new { orderId = order.Id });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EnableRefundOverride(int orderId, string note)
        {
            var order = await _context.Orders
                .Include(o => o.FraudFlags)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            order.RefundOverrideEnabled = true;
            order.RefundOverrideNote = note;
            order.RefundOverrideAt = DateTime.UtcNow;
            order.RefundOverrideByUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            // 🔴 AUDIT / TIMELINE KAYDI (ASIL EKLENEN KISIM)
            await _timelineService.AddAsync(
                orderId,
                TimelineEventType.Fraud,
                $"Refund override aktif edildi. Not: {note}",
                User.Identity?.Name
            );

            TempData["Success"] = "Refund override aktif edildi.";

            return RedirectToAction("Details", new { id = orderId });
        }

        private async Task<Order?> LoadOrderAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.FraudFlags)
                .Include(o => o.PartialRefunds)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        //private List<ViewModels.Orders.TimelineItemVM> BuildTimeline(Order order, PaymentLog? paymentLog)
        //{
        //    var list = new List<TimelineItemVM>();

        //    list.Add(new TimelineItemVM
        //    {
        //        EventType = TimelineEventType.OrderCreated,
        //        Title = "Sipariş Oluşturuldu",
        //        Description = $"Sipariş #{order.Id}",
        //        Date = order.OrderDate
        //    });

        //    if (paymentLog != null)
        //    {
        //        list.Add(new TimelineItemVM
        //        {
        //            EventType = TimelineEventType.PaymentReceived,
        //            Title = "Ödeme",
        //            Description = paymentLog.PaymentStatus,
        //            Date = paymentLog.CreatedAt
        //        });
        //    }

        //    foreach (var refund in order.PartialRefunds)
        //    {
        //        list.Add(new TimelineItemVM
        //        {
        //            EventType = TimelineEventType.Refund,
        //            Title = "İade",
        //            Description = $"{refund.Quantity} adet iade",
        //            Date = refund.CreatedAt
        //        });
        //    }

        //    // 🚨 Fraud
        //    foreach (var f in order.FraudFlags)
        //    {
        //        list.Add(new TimelineItemVM
        //        {
        //            Date = f.IsResolved && f.ResolvedAt.HasValue
        //                ? f.ResolvedAt.Value
        //                : f.CreatedAt,

        //            Title = f.IsResolved
        //                ? $"Fraud Resolved: {f.RuleCode}"
        //                : $"Fraud Detected: {f.RuleCode}",

        //            Description = f.IsResolved
        //                ? (string.IsNullOrWhiteSpace(f.ResolutionNote)
        //                    ? "Resolved without note"
        //                    : f.ResolutionNote)
        //                : f.Description,

        //            EventType = f.IsResolved
        //                ? TimelineEventType.Fraud
        //                : TimelineEventType.Fraud
        //        });
        //    }
        //    return list
        //        .OrderByDescending(x => x.Date)
        //        .ToList();
        //}

        private List<FraudFlagItemVM> MapFraudFlags(Order order)
        {
            var hitCounts = order.FraudFlags
                .GroupBy(f => f.RuleCode)
                .ToDictionary(g => g.Key, g => g.Count());

            return order.FraudFlags
                .OrderByDescending(f => f.Severity)
                .ThenByDescending(f => f.CreatedAt)
                .Select(f => new FraudFlagItemVM
                {
                    RuleCode = f.RuleCode,
                    Description = f.Description,
                    Severity = f.Severity,
                    IsResolved = f.IsResolved,
                    CreatedAt = f.CreatedAt,
                    HitCount = hitCounts.TryGetValue(f.RuleCode, out var c) ? c : 1
                })
                .ToList();
        }

        private Dictionary<string, int> GetRuleHitCounts(Order order)
        {
            return order.FraudFlags
                .GroupBy(f => f.RuleCode)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        [IgnoreAntiforgeryToken] // şimdilik
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int orderId, string status)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            if (!Enum.TryParse<OrderStatus>(status, out var parsedStatus))
                return BadRequest("Geçersiz status");

            order.Status = parsedStatus;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> CreatePartialRefund(
            int orderItemId,
            int quantity)
        {
            var orderItem = await _context.OrderItems
                .Include(x => x.Order)
                    .ThenInclude(o => o.FraudFlags)
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == orderItemId);

            if (orderItem == null)
                return NotFound();

            var order = orderItem.Order;

            // 🚨 HIGH FRAUD HARD BLOCK
            if (order.FraudFlags.Any(f =>
                    f.Severity == FraudSeverity.High &&
                    !f.IsResolved) &&
                !order.RefundOverrideEnabled)
            {
                TempData["Error"] =
                    "HIGH Fraud bulunan siparişlerde doğrudan iade yapılamaz.";
                return RedirectToAction(
                    "Details",
                    "Orders",
                    new { area = "Admin", id = order.Id }
                );
            }

            var refundable =
                orderItem.Quantity - orderItem.RefundedQuantity;

            if (quantity <= 0 || quantity > refundable)
            {
                TempData["Error"] = "Geçersiz iade miktarı";
                return RedirectToAction(
                    "Details",
                    "Orders",
                    new { area = "Admin", id = order.Id }
                );
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                orderItem.RefundedQuantity += quantity;
                orderItem.Product.Stock += quantity;

                _context.PartialRefunds.Add(new PartialRefund
                {
                    OrderId = order.Id,
                    OrderItemId = orderItem.Id,
                    Quantity = quantity,
                    RefundAmount = quantity * orderItem.Price,
                    CreatedAt = DateTime.UtcNow,
                    Status = RefundStatus.Completed
                });

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = "İade başarıyla yapıldı.";
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["Error"] = "İade sırasında hata oluştu.";
            }

            return RedirectToAction(
                "Details",
                "Orders",
                new { area = "Admin", id = order.Id }
            );
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UndoRefund(int refundId)
        {
            int orderId;

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var refund = await _context.PartialRefunds
                    .Include(r => r.Order)
                        .ThenInclude(o => o.OrderItems)
                    .Include(r => r.OrderItem)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(r => r.Id == refundId);

                if (refund == null)
                    return NotFound();

                orderId = refund.OrderId; // ✅ scope dışına taşı

                refund.OrderItem.RefundedQuantity -= refund.Quantity;
                refund.OrderItem.Product.Stock -= refund.Quantity;

                refund.Status = RefundStatus.Cancelled;
                refund.CancelledAt = DateTime.UtcNow;

                refund.Order.Status =
                    refund.Order.OrderItems.All(x => x.RefundedQuantity == 0)
                        ? OrderStatus.Completed
                        : OrderStatus.PartialRefund;

                await _context.SaveChangesAsync();

                await _timelineService.AddAsync(
                    refund.OrderId,
                    TimelineEventType.UndoRefund,
                    $"{refund.Quantity} adet iade geri alındı.",
                    User.Identity?.Name
                );

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["Error"] = "İade geri alınırken hata oluştu.";
                return RedirectToAction("Details", new { id = refundId }); // fallback
            }

            TempData["Success"] = "İade geri alındı.";
            return RedirectToAction("Details", new { id = orderId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            if (order.Status == status)
                return RedirectToAction(
                    "Details",
                    "Orders",
                    new { area = "Admin", id }
                );

            order.Status = status;

            switch (status)
            {
                case OrderStatus.Shipped:
                    await _timelineService.AddAsync(
                        order.Id,
                        TimelineEventType.Shipped,
                        "Sipariş kargoya verildi.",
                        User.Identity?.Name
                    );
                    break;

                case OrderStatus.Completed:
                    await _timelineService.AddAsync(
                        order.Id,
                        TimelineEventType.Delivered,
                        "Sipariş teslim edildi.",
                        User.Identity?.Name
                    );
                    break;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Orders",
                new { area = "Admin", id }
            );
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            if (order.Status == status)
                return RedirectToAction("Details", new { id });

            order.Status = status;
            await _context.SaveChangesAsync();

            if (status == OrderStatus.Shipped)
            {
                await _timelineService.AddAsync(
                    order.Id,
                    TimelineEventType.Shipped,
                    "Sipariş kargoya verildi.",
                    User.Identity?.Name
                );
            }

            if (status == OrderStatus.Completed)
            {
                await _timelineService.AddAsync(
                    order.Id,
                    TimelineEventType.Delivered,
                    "Sipariş teslim edildi.",
                    User.Identity?.Name
                );
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisableRefundOverride(int orderId, string note)
        {
            var order = await _context.Orders
                .Include(o => o.FraudFlags)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            if (!order.RefundOverrideEnabled)
            {
                TempData["Info"] = "Refund override zaten kapalı.";
                return RedirectToAction("Details", new { id = orderId });
            }

            order.RefundOverrideEnabled = false;
            order.RefundOverrideNote = note;
            order.RefundOverrideAt = DateTime.UtcNow;
            order.RefundOverrideByUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            // 🔴 AUDIT / TIMELINE
            await _timelineService.AddAsync(
                orderId,
                TimelineEventType.Fraud,
                $"Refund override kapatıldı. Not: {note}",
                User.Identity?.Name
            );

            TempData["Success"] = "Refund override kapatıldı.";

            return RedirectToAction("Details", new { id = orderId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResolveFraud(int fraudId, string note)
        {
            var fraud = await _context.FraudFlags
                .Include(f => f.Order)
                .FirstOrDefaultAsync(f => f.Id == fraudId);

            if (fraud == null)
                return NotFound();

            if (fraud.IsResolved)
            {
                TempData["Info"] = "Bu fraud zaten çözüldü.";
                return RedirectToAction("Details", new { id = fraud.OrderId });
            }

            fraud.IsResolved = true;
            fraud.ResolvedAt = DateTime.UtcNow;
            fraud.ResolvedByUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);
            fraud.ResolutionNote = note;

            await _context.SaveChangesAsync();

            // 🔴 AUDIT
            await _timelineService.AddAsync(
                fraud.Order.Id,
                TimelineEventType.Fraud,
                $"Fraud çözüldü ({fraud.Severity}). Not: {note}",
                User.Identity?.Name
            );

            TempData["Success"] = "Fraud başarıyla çözüldü.";

            return RedirectToAction("Details", new { id = fraud.OrderId });
        }

        [HttpPost]
        public async Task<IActionResult> ResolveFraudFlag(int id, string note)
        {
            var flag = await _context.FraudFlags.FindAsync(id);

            if (flag == null)
                return NotFound();

            flag.IsResolved = true;
            flag.ResolutionNote = note;
            flag.ResolvedAt = DateTime.UtcNow;

            var user = await _userManager.GetUserAsync(User);

            if (user != null)
                flag.ResolvedByUserId = user.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = flag.OrderId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFraudCaseStatus(int id, Modules.Fraud.Models.FraudCaseStatus status)
        {
            var flag = await _context.FraudFlags.FindAsync(id);

            if (flag == null)
                return NotFound();

            flag.CaseStatus = status;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = flag.OrderId });
        }
    }
}