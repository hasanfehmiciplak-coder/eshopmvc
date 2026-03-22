namespace EShopMVC.Modules.Payments.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string GatewayTransactionId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaidAt { get; set; }

        public bool Processed { get; set; }
    }
}