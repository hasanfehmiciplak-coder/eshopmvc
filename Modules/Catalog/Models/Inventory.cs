namespace EShopMVC.Modules.Catalog.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int VariantId { get; set; }

        public int StockQuantity { get; set; }

        public int ReservedQuantity { get; set; }

        public int AvailableQuantity { get; set; }

        public void Reserve(int quantity)
        {
            if (AvailableQuantity < quantity)
                throw new Exception("Insufficient stock");

            ReservedQuantity += quantity;
        }
    }
}