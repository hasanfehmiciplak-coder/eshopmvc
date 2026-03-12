namespace EShopMVC.Areas.Admin.ViewModels
{
    public class FraudGraphVM
    {
        public string UserId { get; set; }

        public List<int> Orders { get; set; } = new();

        public List<string> IpAddresses { get; set; } = new();

        public int RefundCount { get; set; }
    }
}