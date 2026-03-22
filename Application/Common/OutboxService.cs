using System.Text.Json;
using EShopMVC.Domain.Base;
using EShopMVC.Domain.Entities;
using EShopMVC.Shared.Events;

public class OutboxService
{
    public List<OutboxMessage> CreateMessages(IEnumerable<DomainEvent> events)
    {
        return events.Select(e => new OutboxMessage
        {
            Type = e.GetType().Name,
            Payload = JsonSerializer.Serialize(e),
            OccurredOn = DateTime.UtcNow
        }).ToList();
    }
}