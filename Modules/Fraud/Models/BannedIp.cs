namespace EShopMVC.Modules.Fraud.Models
{
    public class BannedIp
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string Reason { get; set; }

        public DateTime BannedAt { get; set; } = DateTime.Now;
        public string BannedByUserId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}