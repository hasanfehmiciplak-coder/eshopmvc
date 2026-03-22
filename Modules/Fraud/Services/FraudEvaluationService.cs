using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Domain.Enums;
using Order = EShopMVC.Modules.Orders.Domain.Entities.Order;
using Refund = EShopMVC.Modules.Orders.Domain.Entities.Refund;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudEvaluationService : IFraudEvaluationService
    {
        private readonly FraudRuleEngine _ruleEngine;
        private readonly FraudRiskPipeline _riskPipeline;
        private readonly FraudTimelineService _timeline;
        private readonly FraudCaseService _caseService;
        private readonly FraudAutoBlockService _autoBlock;
        private readonly FraudAlertService _alertService;

        public FraudEvaluationService(
            FraudRuleEngine ruleEngine,
            FraudRiskPipeline riskPipeline,
            FraudTimelineService timeline,
            FraudCaseService caseService,
            FraudAutoBlockService autoBlock,
            FraudAlertService alertService)
        {
            _ruleEngine = ruleEngine;
            _riskPipeline = riskPipeline;
            _timeline = timeline;
            _caseService = caseService;
            _autoBlock = autoBlock;
            _alertService = alertService;
        }

        public async Task<FraudFlag?> EvaluateRefund(Order order, Refund refund)
        {
            var risk = await _riskPipeline.CalculateAsync(order.Id);

            await _timeline.AddRiskScoreEvent(order.Id, risk.TotalScore);

            if (risk.TotalScore > 70)
            {
                await _caseService.CreateCaseAsync(
                    order.Id,
                    order.UserId,
                    FraudSeverity.High
                );

                await _timeline.AddFraudFlagEvent(order.Id, "HIGH_RISK_SCORE");

                await _autoBlock.HandleHighRiskAsync(order, risk.TotalScore);

                if (risk.TotalScore > 90)
                {
                    await _alertService.CreateAlertAsync(order.Id, risk.TotalScore);
                }

                return CreateFlag(
                    order.Id,
                    "RISK_SCORE_HIGH",
                    FraudSeverity.High,
                    $"Toplam risk skoru çok yüksek ({risk.TotalScore})"
                );
            }

            // 1️⃣ FAST REFUND
            if (order.PaidAt.HasValue &&
                (refund.CreatedAt - order.PaidAt.Value).TotalMinutes < 10)
            {
                return CreateFlag(
                    order.Id,
                    "REFUND_FAST",
                    FraudSeverity.Medium,
                    "Ödeme sonrası çok kısa sürede iade alındı"
                );
            }

            // 2️⃣ HIGH RATIO
            var totalRefunded = order.PartialRefunds
                .Where(r => r.Status == RefundStatus.Success)
                .Sum(r => r.Amount) + refund.Amount;

            if (totalRefunded / order.TotalPrice >= 0.8m)
            {
                return CreateFlag(
                    order.Id,
                    "REFUND_HIGH_RATIO",
                    FraudSeverity.High,
                    "Sipariş bedelinin %80'inden fazlası iade edildi"
                );
            }

            // 3️⃣ MULTI REFUND
            var refundCount = order.PartialRefunds
                .Count(r => r.Status == RefundStatus.Success);

            if (refundCount >= 3)
            {
                return CreateFlag(
                    order.Id,
                    "REFUND_MULTI",
                    FraudSeverity.Medium,
                    "Aynı siparişte çok sayıda iade yapıldı"
                );
            }

            return null;
        }

        // ✅ Burada constructor yerine factory metod tanımlıyoruz
        private FraudFlag CreateFlag(
            int orderId,
            string ruleCode,
            FraudSeverity severity,
            string description)
        {
            return new FraudFlag(orderId, ruleCode, severity, description);
        }

        public async Task EvaluateOrderAsync(Order order)
        {
            var risk = await _riskPipeline.CalculateAsync(order.Id);

            await _timeline.AddRiskScoreEvent(order.Id, risk.TotalScore);

            if (risk.TotalScore > 70)
            {
                await _caseService.CreateCaseAsync(
                    order.Id,
                    order.UserId,
                    FraudSeverity.High
                );
            }
        }
    }
}