namespace EShopMVC.Models
{
    public class Firm
    {
        public int Id { get; set; }
        public int FirmId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}