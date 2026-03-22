namespace EShopMVC.Shared.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Payload { get; set; }

        public DateTime OccurredOn { get; set; }
        public bool Processed { get; set; }

        public DateTime? ProcessedOn { get; set; }
    }
}