using EShopMVC.Areas.Admin.ViewModels.Orders;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Models.TimeLine;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Logs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class OrderDetailsService : IOrderDetailsService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public OrderDetailsService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private static string GetOrderDetailsCacheKey(int orderId)
        => $"order_details_{orderId}";

    public async Task<OrderDetailViewModel?> GetOrderDetailsAsync(int orderId)
    {
        var cacheKey = GetOrderDetailsCacheKey(orderId);

        if (_cache.TryGetValue(cacheKey, out OrderDetailViewModel vm))
            return vm;

        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.FraudFlags)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return null;

        var paymentLog = await _context.PaymentLogs
            .FirstOrDefaultAsync(p => p.OrderId == orderId);

        vm = new OrderDetailViewModel
        {
            Order = order,
            FraudFlags = MapFraudFlags(order),
            Timeline = BuildTimeline(order, paymentLog)
        };
        _cache.Set(cacheKey, vm, TimeSpan.FromMinutes(3));

        return vm;
    }

    // ================= TIMELINE =================

    private List<TimelineItemVM> BuildTimeline(
        Order order,
        PaymentLog? paymentLog)
    {
        var list = new List<TimelineItemVM>();

        // 🟢 Sipariş oluşturuldu
        list.Add(new TimelineItemVM
        {
            Date = order.OrderDate,
            Title = "Sipariş Oluşturuldu",
            Description = $"Sipariş #{order.Id}",
            EventType = TimelineEventType.OrderCreated
        });

        // 🚨 Fraud (şimdilik aynı kalabilir)
        foreach (var f in order.FraudFlags)
        {
            list.Add(new TimelineItemVM
            {
                Date = f.IsResolved && f.ResolvedAt.HasValue
                    ? f.ResolvedAt.Value
                    : f.CreatedAt,

                Title = f.IsResolved
                    ? $"Fraud Resolved: {f.RuleCode}"
                    : $"Fraud Detected: {f.RuleCode}",

                Description = f.IsResolved
                    ? (string.IsNullOrWhiteSpace(f.ResolutionNote)
                        ? "Resolved without note"
                        : f.ResolutionNote)
                    : f.Description,

                EventType = TimelineEventType.Fraud // veya Resolved
            });
        }

        // 🕒 Refund / UndoRefund / Payment timeline
        var timelineEvents = _context.OrderTimelines
            .Where(t => t.OrderId == order.Id)
            .OrderBy(t => t.CreatedAt)
            .ToList();

        foreach (var t in timelineEvents)
        {
            list.Add(new TimelineItemVM
            {
                Date = t.CreatedAt,
                Title = t.EventType.ToString(),
                Description = t.Details,
                EventType = t.EventType
            });
        }

        // 💳 Ödeme (istersen bunu da timeline tablosuna taşıyabiliriz)
        if (paymentLog != null)
        {
            list.Add(new TimelineItemVM
            {
                Date = paymentLog.CreatedAt,
                Title = "Payment",
                Description = paymentLog.Status,
                EventType = TimelineEventType.PaymentReceived
            });
        }

        return list
            .OrderByDescending(x => x.Date)
            .ToList();
    }

    // ================= FRAUD FLAGS =================

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
}