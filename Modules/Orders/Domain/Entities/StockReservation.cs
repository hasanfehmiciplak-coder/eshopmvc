namespace EShopMVC.Modules.Orders.Domain.Entities
{
    public class StockReservation
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int VariantId { get; set; }

        public int Quantity { get; set; }

        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        public bool Released { get; set; }
    }
}