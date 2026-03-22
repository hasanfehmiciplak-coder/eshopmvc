using EShopMVC.Domain.Base;
using EShopMVC.Shared.Events;

namespace EShopMVC.Domain.Interfaces
{
    public interface IDomainEventHandler<T> where T : DomainEvent
    {
        Task Handle(T domainEvent);
    }
}