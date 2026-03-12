using DocumentFormat.OpenXml.Drawing.Charts;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Infrastructure.Jobs;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Models.TimeLine;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Fraud.Services;
using EShopMVC.Repositories.Refunds;
using EShopMVC.Services;
using EShopMVC.Services.Refunds;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;

public class RefundService : IRefundService
{
    private readonly IRefundRepository _refundRepository;
    private readonly IPaymentRefundGateway _paymentGateway;
    private readonly IFraudService _fraudService;
    private readonly ILogger<RefundService> _logger;
    private readonly AppDbContext _context;
    private readonly OrderTimelineService _timelineService;
    private readonly FraudDetectionService _fraudDetectionService;

    public RefundService(
        IRefundRepository refundRepository,
        IPaymentRefundGateway paymentGateway,
        IFraudService fraudService,
        ILogger<RefundService> logger,
        AppDbContext context,
        OrderTimelineService timelineService,
        FraudDetectionService fraudDetectionService)
    {
        _refundRepository = refundRepository;
        _paymentGateway = paymentGateway;
        _fraudService = fraudService;
        _logger = logger;
        _context = context;
        _timelineService = timelineService;
        _fraudDetectionService = fraudDetectionService;
    }

    public async Task CreateRefundAsync(Guid refundId)
    {
        var refund = await _refundRepository.GetAsync(refundId);

        _logger.LogInformation(
            "Refund initial attempt started. RefundId={RefundId}",
            refund.Id);

        try
        {
            await _paymentGateway.RefundAsync(refund);

            refund.Status = RefundStatus.Completed;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Refund initial attempt failed. RefundId={RefundId}",
                refund.Id);

            refund.Status = RefundStatus.Failed;
            refund.RetryCount = 0;
            refund.NextRetryAt = null;

            await _refundRepository.UpdateAsync(refund);

            BackgroundJob.Enqueue<RefundRetryJob>(
                job => job.ExecuteAsync(refund.Id));

            throw;
        }

        await _refundRepository.UpdateAsync(refund);

        // TIMELINE
        await _timelineService.AddAsync(
            refund.OrderId,
            TimelineEventType.RefundRequested,
            "Refund işlemi tamamlandı",
            "SYSTEM"
        );

        // FRAUD RULE ENGINE
        await _fraudDetectionService.CheckRulesAsync(refund.OrderId);
    }

    public async Task RetryRefundAsync(Guid refundId)
    {
        var refund = await _refundRepository.GetAsync(refundId);

        if (refund == null || refund.Status != RefundStatus.Failed)
            return;

        _logger.LogInformation(
            "Refund retry attempt {Retry}/{MaxRetry}. RefundId={RefundId}",
            refund.RetryCount + 1,
            refund.MaxRetryCount,
            refund.Id);

        if (refund.RetryCount >= refund.MaxRetryCount)
        {
            refund.Status = RefundStatus.PermanentFailed;
            refund.NextRetryAt = null;

            await _refundRepository.UpdateAsync(refund);

            _logger.LogError(
                "Refund permanently failed. RefundId={RefundId}, TotalRetries={RetryCount}",
                refund.Id,
                refund.RetryCount);

            _fraudService.FlagRefund(new FraudFlag
            {
                RefundId = refund.Id,
                Reason = FraudReason.RefundRetryExceeded,
                RuleCode = "REFUND_RETRY_EXCEEDED",
                Description = "Refund maksimum retry sayısını aştı.",
                Severity = FraudSeverity.High
            });

            return;
        }

        try
        {
            await _paymentGateway.RefundAsync(refund);
            refund.Status = RefundStatus.Completed;
            refund.NextRetryAt = null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Refund retry failed. RefundId={RefundId}, RetryCount={RetryCount}",
                refund.Id,
                refund.RetryCount);

            refund.RetryCount++;

            refund.NextRetryAt = DateTime.UtcNow.AddMinutes(
                Math.Pow(2, refund.RetryCount));

            await _refundRepository.UpdateAsync(refund);

            BackgroundJob.Schedule<RefundRetryJob>(
                job => job.ExecuteAsync(refund.Id),
                refund.NextRetryAt.Value);

            return;
        }
    }

    public async Task UndoRefundAsync(
        Guid refundId,
        string reason,
        string? performedBy)
    {
        var refund = await _context.Refunds
            .FirstOrDefaultAsync(r => r.Id == refundId);

        if (refund == null)
            throw new Exception("Refund not found");

        if (refund.Status != RefundStatus.Pending)
            throw new Exception("Only pending refunds can be undone");

        var orderItem = await _context.OrderItems
            .Include(x => x.Order)
                .ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(oi => oi.Id == refund.OrderItemId);

        if (orderItem == null)
            throw new Exception("Order item not found");

        orderItem.RefundedQuantity -= refund.Quantity;

        refund.Status = RefundStatus.Cancelled;
        refund.CancelledAt = DateTime.UtcNow;

        // 🔴 STATUS GERİ HESAPLA
        orderItem.Order.Status =
            OrderStatusCalculator.Calculate(orderItem.Order);

        await _context.SaveChangesAsync();

        await _timelineService.AddAsync(
            refund.OrderId,
            TimelineEventType.UndoRefund,
            $"Refund geri alındı. Adet: {refund.Quantity}, Tutar: {refund.Amount} ₺",
            performedBy
        );
    }
}