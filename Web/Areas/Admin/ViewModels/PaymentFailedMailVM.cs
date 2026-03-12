namespace EShopMVC.Areas.Admin.ViewModels
{
    public class PaymentFailedMailVM
    {
        public int OrderId { get; set; }
        public string UserEmail { get; set; } = null!;
        public string? ErrorMessage { get; set; }
    }
}