namespace EShopMVC.Models.Dtos
{
    public class IyzicoRefundResult
    {
        public string Status { get; set; }   // success / failure
        public string Raw { get; set; }      // iyzico raw response
    }
}