using EShopMVC.Domain.Base;
using EShopMVC.Domain.Interfaces;
using EShopMVC.Shared.Events;

namespace EShopMVC.Application.Common
{
    public class DomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainEventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchAsync(IEnumerable<DomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                var handlerType = typeof(IDomainEventHandler<>)
                    .MakeGenericType(domainEvent.GetType());

                var handlers = _serviceProvider.GetServices(handlerType);

                foreach (var handler in handlers)
                {
                    var method = handlerType.GetMethod("Handle");

                    if (method != null)
                        await (Task)method.Invoke(handler, new object[] { domainEvent });
                }
            }
        }
    }
}