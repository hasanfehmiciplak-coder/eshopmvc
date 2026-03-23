namespace EShopMVC.Modules.Orders.Domain.Logs
{
    public class OrderLog
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public string Description { get; set; } = null!;

        public string Action { get; set; }

        public string User { get; set; }

        public string EventType { get; set; }
        public string? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}