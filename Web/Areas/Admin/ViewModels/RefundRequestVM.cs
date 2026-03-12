using System.ComponentModel.DataAnnotations;

namespace EShopMVC.Areas.Admin.ViewModels
{
    public class RefundRequestVM
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int OrderItemId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}