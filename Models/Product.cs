using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShopMVC.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public int FirmId { get; set; }
        public Firm? Firm { get; set; }

        public string? Slug { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; } // opsiyonel, ileride resim upload için

        public bool IsActive { get; set; } = true;
        public int Stock { get; set; }

        public int CriticalStock { get; set; } = 5;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}