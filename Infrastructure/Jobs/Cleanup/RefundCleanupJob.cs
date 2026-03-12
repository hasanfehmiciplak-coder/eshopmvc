using EShopMVC.Repositories.Refunds;

public class RefundCleanupJob
{
    private readonly IRefundRepository _refundRepository;
    private readonly ILogger<RefundCleanupJob> _logger;

    public RefundCleanupJob(
        IRefundRepository refundRepository,
        ILogger<RefundCleanupJob> logger)
    {
        _refundRepository = refundRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var deletedCount =
            await _refundRepository.DeleteCompletedOlderThanAsync(
                DateTime.UtcNow.AddMonths(-6));

        _logger.LogInformation(
            "Refund cleanup executed. Deleted {Count} records.",
            deletedCount);
    }
}