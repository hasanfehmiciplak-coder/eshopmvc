using EShopMVC.Modules.Orders.Application.Services;
using Hangfire;

namespace EShopMVC.Infrastructure.Jobs
{
    public class RefundRetryJob
    {
        private readonly RefundService _refundService;

        public RefundRetryJob(RefundService refundService)
        {
            _refundService = refundService;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task ExecuteAsync(int refundId)
        {
            await _refundService.RetryRefundAsync(refundId);
        }
    }
}