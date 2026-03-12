using EShopMVC.Infrastructure.Data;
using EShopMVC.Infrastructure.Jobs;
using EShopMVC.Models.TimeLine;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;

public class JobFailureTimelineFilter : IElectStateFilter
{
    private readonly IServiceScopeFactory _scopeFactory;

    public JobFailureTimelineFilter(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void OnStateElection(ElectStateContext context)
    {
        // ❗ Failed state mi?
        if (context.CandidateState is not FailedState failedState)
            return;

        // ❗ Sadece OrderMailJob
        if (context.BackgroundJob.Job.Type != typeof(OrderMailJob))
            return;

        var orderId = (int)context.BackgroundJob.Job.Args[0];

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.OrderTimelines.Add(new OrderTimeline
        {
            OrderId = orderId,
            EventType = TimelineEventType.Warning,
            Description = "Sipariş emaili gönderilemedi",
            Details = failedState.Exception?.Message ?? "Bilinmeyen hata",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM"
        });

        db.SaveChanges();
    }

    public void OnStateApplied(
        ApplyStateContext context,
        IWriteOnlyTransaction transaction)
    { }

    public void OnStateUnapplied(
        ApplyStateContext context,
        IWriteOnlyTransaction transaction)
    { }
}