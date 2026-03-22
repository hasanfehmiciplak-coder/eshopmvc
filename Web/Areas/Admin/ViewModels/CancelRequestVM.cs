namespace EShopMVC.Web.Areas.Admin.ViewModels
{
    public class CancelRequestVM
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}