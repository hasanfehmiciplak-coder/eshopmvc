using System.ComponentModel.DataAnnotations.Schema;
using EShopMVC.Shared.Events;

namespace EShopMVC.Shared.Domain
{
    public abstract class BaseEntity
    {
        public int Id { get; protected set; }

        private readonly List<DomainEvent> _domainEvents = new();

        [NotMapped]
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(DomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}