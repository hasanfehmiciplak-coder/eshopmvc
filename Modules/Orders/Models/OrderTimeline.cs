using EShopMVC.Models.TimeLine;

public class OrderTimeline
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public TimelineEventType EventType { get; set; }

    public string Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? PerformedByUserId { get; set; }

    public string? PerformedByUserName { get; set; }
    public string? Details { get; set; } = null!;

    public string? CreatedBy { get; set; }
}