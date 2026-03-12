namespace EShopMVC.Areas.Admin.ViewModels
{
    public class OrderSuccessMailVM
    {
        public int OrderId { get; set; }
        public string UserEmail { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
    }
}