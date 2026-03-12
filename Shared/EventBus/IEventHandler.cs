using EShopMVC.Shared.Events;

namespace EShopMVC.Shared.EventBus
{
    public interface IEventHandler<TEvent> where TEvent : DomainEvent
    {
        Task HandleAsync(TEvent domainEvent);
    }
}