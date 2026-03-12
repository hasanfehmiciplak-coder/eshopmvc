namespace EShopMVC.Web.ViewModels
{
    public class CheckoutReviewVM
    {
        public int AddressId { get; set; }      // POST için
        public string AddressText { get; set; } // ekranda gösterim
        public List<CheckoutItemVM> Items { get; set; }
        public decimal Total => Items.Sum(x => x.Price * x.Quantity);
    }

    public class CheckoutItemVM
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}