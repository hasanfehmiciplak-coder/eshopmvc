namespace EShopMVC.Web.ViewModels
{
    public class CreatePartialRefundVM
    {
        public int OrderId { get; set; }

        public List<RefundItemVM> Items { get; set; } = new();
    }

    public class RefundItemVM
    {
        public int OrderItemId { get; set; }
        public string ProductName { get; set; }

        public int OrderedQuantity { get; set; }
        public int AlreadyRefundedQuantity { get; set; }

        public int RefundQuantity { get; set; } // admin girer
        public decimal UnitPrice { get; set; }

        public string Reason { get; set; }
    }
}