using EShopMVC.Shared.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EShopMVC.Shared.EventBus
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;

        public InMemoryEventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : DomainEvent
        {
            var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();

            foreach (var handler in handlers)
            {
                await handler.HandleAsync(domainEvent);
            }
        }
    }
}