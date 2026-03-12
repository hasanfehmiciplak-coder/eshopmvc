namespace EShopMVC.Modules.Fraud.Models
{
    public class RiskScoreResult
    {
        public int RuleScore { get; set; }

        public int UserScore { get; set; }

        public int RefundScore { get; set; }

        public int IpScore { get; set; }

        public int BehaviorScore { get; set; }

        public int TotalScore =>
            RuleScore +
            UserScore +
            RefundScore +
            IpScore +
            BehaviorScore;
    }
}