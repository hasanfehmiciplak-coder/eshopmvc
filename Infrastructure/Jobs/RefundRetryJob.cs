using EShopMVC.Services.Refunds;
using Hangfire;

namespace EShopMVC.Infrastructure.Jobs
{
    public class RefundRetryJob
    {
        private readonly IRefundService _refundService;

        public RefundRetryJob(IRefundService refundService)
        {
            _refundService = refundService;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task ExecuteAsync(Guid refundId)
        {
            await _refundService.RetryRefundAsync(refundId);
        }
    }
}