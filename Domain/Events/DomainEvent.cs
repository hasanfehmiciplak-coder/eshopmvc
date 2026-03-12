namespace EShopMVC.Domain.Events
{
    public abstract class DomainEvent
    {
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }
}