using EShopMVC.Models.TimeLine;

namespace EShopMVC.Areas.Admin.ViewModels.Orders
{
    public class TimelineItemVM
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TimelineEventType EventType { get; set; }

        public string? PerformedBy { get; set; }

        public string Icon { get; set; }
        public string IconCss { get; set; }

        public string? Details { get; set; }

        public string FraudDetails { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsRetry =>
    !string.IsNullOrEmpty(Title) &&
    Title.Contains("Retry", StringComparison.OrdinalIgnoreCase);

        public string EffectiveIcon =>
    IsRetry ? "fa-solid fa-rotate-right" : Icon;

        public string EffectiveIconCss =>
            IsRetry ? "bg-warning text-dark" : IconCss;

        // 🔁 Retry bilgisi
        public int? RetryCount { get; set; }

        // 🔍 Filtreleme için
        public string GroupKey { get; set; } // Payment, Refund, Fraud, System

        // 🎨 UI severity
        public TimelineSeverity Severity { get; set; }
    }

    public enum TimelineSeverity
    {
        Info,
        Success,
        Warning,
        Danger
    }
}