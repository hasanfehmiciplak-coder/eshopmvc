namespace EShopMVC.Web.Areas.Admin.ViewModels
{
    public class OrderCancelRequestVM
    {
        public int Id { get; set; }

        public string CustomerEmail { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }
    }
}