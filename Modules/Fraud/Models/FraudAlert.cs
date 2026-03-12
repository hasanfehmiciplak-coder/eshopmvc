namespace EShopMVC.Modules.Fraud.Models
{
    public class FraudAlert
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string Message { get; set; }

        public int RiskScore { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}