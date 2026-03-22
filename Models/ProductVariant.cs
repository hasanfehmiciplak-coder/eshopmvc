using System.ComponentModel.DataAnnotations;

namespace EShopMVC.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required]
        public string Size { get; set; }

        [Required]
        public string Color { get; set; }

        public int Stock { get; set; }

        public string Sku { get; set; }

        public Product Product { get; set; }

        public ICollection<ProductVariantAttribute> Attributes { get; set; }
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}