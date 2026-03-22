using System.ComponentModel.DataAnnotations.Schema;

namespace EShopMVC.Shared.Events
{
    [NotMapped]
    public abstract class DomainEvent
    {
        public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
    }
}