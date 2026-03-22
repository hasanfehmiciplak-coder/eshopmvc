namespace EShopMVC.Models
{
    public class ProductVariantAttribute
    {
        public int Id { get; set; }

        public int ProductVariantId { get; set; }

        public string AttributeName { get; set; }

        public string AttributeValue { get; set; }

        public ProductVariant ProductVariant { get; set; }
    }
}