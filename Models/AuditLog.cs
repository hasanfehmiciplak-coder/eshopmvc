namespace EShopMVC.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string UserId { get; set; }   // null olabilir
        public string Email { get; set; }

        public string Action { get; set; }   // LOGIN_SUCCESS, LOGIN_FAIL, LOGOUT...
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}