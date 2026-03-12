namespace EShopMVC.Areas.Admin.ViewModels.Orders
{
    public class AdminOrderListVM
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }

        public bool HasHighFraud { get; set; }
        public bool RefundOverrideEnabled { get; set; }
    }
}