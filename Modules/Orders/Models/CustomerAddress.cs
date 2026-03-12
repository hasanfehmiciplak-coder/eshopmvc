using EShopMVC.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShopMVC.Modules.Orders.Models
{
    public class CustomerAddress
    {
        public int Id { get; set; }

        // 🔗 Kullanıcı
        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        // 📌 Adres Bilgileri

        [Required(ErrorMessage = "Adres başlığı zorunludur")]
        public string Title { get; set; }   // Ev, İş vb.

        [Required(ErrorMessage = "Adres zorunludur")]
        public string FullAddress =>
            $"{AddressLine}, {District} / {City}";

        public string FullName { get; set; }

        [Required]
        public string City { get; set; }

        public string District { get; set; }

        public string AddressLine { get; set; }

        // ➕ Eklenen Alanlar
        public string Phone { get; set; }

        public string PostalCode { get; set; }

        // ⭐ Varsayılan adres
        public bool IsDefault { get; set; }

        public string Street { get; set; }
        public string Detail { get; set; }
    }
}