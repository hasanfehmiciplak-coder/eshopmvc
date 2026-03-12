using EShopMVC.Modules.Orders.Models;
using System.ComponentModel.DataAnnotations;

namespace EShopMVC.Web.ViewModels
{
    public class CheckoutVM
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal Total { get; set; }

        //public string CustomerName { get; set; }

        [Required(ErrorMessage = "Adres seçmelisiniz")]
        public int AddressId { get; set; }

        //public List<CustomerAddress> Addresses { get; set; }
        public int? SelectedAddressId { get; set; }

        //public Cart Cart { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }
    }
}