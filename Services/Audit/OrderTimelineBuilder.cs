using EShopMVC.Areas.Admin.ViewModels.Orders;
using EShopMVC.Models.Fraud;
using EShopMVC.Models.TimeLine;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Models;

namespace EShopMVC.Services.Orders
{
    public class OrderTimelineBuilder
    {
        public List<TimelineItemVM> Build(
            Order order,
            IEnumerable<OrderTimeline> timelines,
            IEnumerable<FraudFlag> frauds)
        {
            var list = new List<TimelineItemVM>();

            // ==============================
            // ORDER TIMELINES (Retry logic burada)
            // ==============================
            var groupedTimelines = timelines
                .OrderBy(t => t.CreatedAt)
                .GroupBy(t => t.EventType);

            foreach (var group in groupedTimelines)
            {
                var retryIndex = 1;

                foreach (var timeline in group)
                {
                    var (icon, css) = ResolveIcon(timeline.EventType);

                    list.Add(new TimelineItemVM
                    {
                        Date = timeline.CreatedAt,

                        Title = retryIndex > 1
                            ? $"{GetTitle(timeline.EventType)} (Retry #{retryIndex})"
                            : GetTitle(timeline.EventType),

                        Description = timeline.Details,
                        EventType = timeline.EventType,
                        Icon = icon,
                        IconCss = css,
                        PerformedBy = timeline.PerformedByUserName,

                        RetryCount = retryIndex > 1 ? retryIndex : null,
                        GroupKey = ResolveGroupKey(timeline.EventType),
                        Severity = ResolveSeverity(timeline.EventType),

                        Details = timeline.EventType == TimelineEventType.Refund
                                || timeline.EventType == TimelineEventType.UndoRefund
                                ? timeline.Details
                                : null
                    });

                    retryIndex++;
                }
            }

            // ==============================
            // FRAUD FLAGS (system events)
            // ==============================
            foreach (var fraud in frauds)
            {
                list.Add(new TimelineItemVM
                {
                    Date = fraud.CreatedAt,
                    Title = "🚨 Fraud Detected",
                    Description = fraud.Description,
                    EventType = TimelineEventType.Fraud,
                    Icon = "bi bi-shield-exclamation",
                    IconCss = "bg-danger",
                    PerformedBy = "SYSTEM",

                    GroupKey = "Fraud",
                    Severity = TimelineSeverity.Danger,
                    FraudDetails = fraud.Reason.ToString()
                });
            }

            return list
                .OrderByDescending(x => x.Date)
                .ToList();
        }

        // ==============================
        // HELPERS
        // ==============================

        private string GetTitle(TimelineEventType type)
        {
            return type switch
            {
                TimelineEventType.OrderCreated => "Sipariş Oluşturuldu",
                TimelineEventType.PaymentInitiated => "Ödeme Başlatıldı",
                TimelineEventType.PaymentReceived => "Ödeme Alındı",
                TimelineEventType.PaymentFailed => "Ödeme Başarısız",
                TimelineEventType.Refund => "İade Yapıldı",
                TimelineEventType.UndoRefund => "İade Geri Alındı",
                TimelineEventType.Shipped => "Kargoya Verildi",
                TimelineEventType.Delivered => "Teslim Edildi",
                _ => type.ToString()
            };
        }

        private (string icon, string css) ResolveIcon(TimelineEventType type)
        {
            return type switch
            {
                TimelineEventType.PaymentInitiated
                    => ("bi bi-credit-card", "bg-info"),

                TimelineEventType.OrderCreated
                    => ("bi bi-receipt", "bg-secondary"),

                TimelineEventType.PaymentFailed
                    => ("bi bi-x-circle", "bg-danger"),

                TimelineEventType.PaymentReceived
                    => ("bi bi-credit-card-check", "bg-success"),

                TimelineEventType.Shipped
                    => ("bi bi-truck", "bg-info"),

                TimelineEventType.Delivered
                    => ("bi bi-box-seam", "bg-success"),

                TimelineEventType.Refund
                    => ("bi bi-arrow-counterclockwise", "bg-warning"),

                TimelineEventType.UndoRefund
                    => ("bi bi-arrow-counterclockwise", "bg-secondary"),

                TimelineEventType.Fraud
                    => ("bi bi-shield-exclamation", "bg-danger"),

                TimelineEventType.Debug
                    => ("bi bi-bug", "bg-dark"),

                _ => ("bi bi-dot", "bg-light")
            };
        }

        private string ResolveGroupKey(TimelineEventType type)
        {
            return type switch
            {
                TimelineEventType.PaymentReceived => "Payment",
                TimelineEventType.Refund => "Refund",
                TimelineEventType.UndoRefund => "Refund",
                TimelineEventType.Fraud => "Fraud",
                TimelineEventType.PaymentFailed => "Payment",
                _ => "System"
            };
        }

        private TimelineSeverity ResolveSeverity(TimelineEventType type)
        {
            return type switch
            {
                TimelineEventType.PaymentReceived => TimelineSeverity.Success,
                TimelineEventType.Refund => TimelineSeverity.Warning,
                TimelineEventType.UndoRefund => TimelineSeverity.Info,
                TimelineEventType.Fraud => TimelineSeverity.Danger,
                _ => TimelineSeverity.Info
            };
        }
    }
}