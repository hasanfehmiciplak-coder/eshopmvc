using EShopMVC.Areas.Admin.ViewModels;
using EShopMVC.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FraudSimulationController : Controller
    {
        private readonly AppDbContext _context;

        public FraudSimulationController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new FraudSimulationVM());
        }

        [HttpPost]
        public async Task<IActionResult> Index(FraudSimulationVM model)
        {
            var rules = await _context.FraudRules
                .Where(x => x.IsActive)
                .ToListAsync();

            int score = 0;

            foreach (var rule in rules)
            {
                if (rule.RuleType == "Amount" &&
                    model.OrderAmount >= rule.Threshold)
                {
                    score += rule.RiskScore;
                    model.TriggeredRules.Add(rule.Name);
                }

                if (rule.RuleType == "Refund" &&
                    model.RefundCount >= rule.Threshold)
                {
                    score += rule.RiskScore;
                    model.TriggeredRules.Add(rule.Name);
                }
            }

            model.RiskScore = score;

            return View(model);
        }
    }
}