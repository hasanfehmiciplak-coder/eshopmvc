using EShopMVC.Models.TimeLine;

public class OrderTimeline
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public TimelineEventType EventType { get; set; } // ✔ enum oldu

    public string? Details { get; set; }

    public string? PerformedByUserName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Description { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;
}