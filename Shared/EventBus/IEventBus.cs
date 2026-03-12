using EShopMVC.Shared.Events;

namespace EShopMVC.Shared.EventBus
{
    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : DomainEvent;
    }
}