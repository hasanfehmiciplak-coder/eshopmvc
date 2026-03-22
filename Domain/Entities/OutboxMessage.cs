using EShopMVC.Shared.Domain;

namespace EShopMVC.Domain.Entities
{
    public class OutboxMessage : BaseEntity
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Payload { get; set; }

        public DateTime OccurredOn { get; set; }

        public DateTime? ProcessedOn { get; set; }

        public int RetryCount { get; set; }

        public string? Error { get; set; }
    }
}