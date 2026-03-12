using EShopMVC.Modules.Fraud.Models;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudRiskPipeline
    {
        private readonly FraudRuleEngine _ruleEngine;
        private readonly FraudScoreService _scoreService;
        private readonly UserFraudService _userFraudService;
        private readonly BehaviorScoreService _behaviorService;

        public FraudRiskPipeline(
            FraudRuleEngine ruleEngine,
            FraudScoreService scoreService,
            UserFraudService userFraudService,
            BehaviorScoreService behaviorService)
        {
            _ruleEngine = ruleEngine;
            _scoreService = scoreService;
            _userFraudService = userFraudService;
            _behaviorService = behaviorService;
        }

        public async Task<RiskScoreResult> CalculateAsync(int orderId)
        {
            var result = new RiskScoreResult();

            result.RuleScore =
                await _ruleEngine.CalculateRiskScoreAsync(orderId);

            result.UserScore =
                await _userFraudService.GetUserRiskScore(orderId);

            result.RefundScore =
                await _scoreService.CalculateRefundScore(orderId);

            result.IpScore =
                await _scoreService.CalculateIpScore(orderId);

            result.BehaviorScore =
                 await _behaviorService.CalculateBehaviorScore(orderId);

            return result;
        }
    }
}