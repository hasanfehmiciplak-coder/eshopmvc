namespace EShopMVC.Modules.Orders.Models
{
    public class OrderLog
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string Action { get; set; } = null!;
        public string Description { get; set; } = null!;

        // 👇 USER görebilir mi?
        public bool VisibleToUser { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}