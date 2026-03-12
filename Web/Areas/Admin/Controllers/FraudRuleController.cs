using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Fraud.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FraudRuleController : Controller
    {
        private readonly AppDbContext _context;

        public FraudRuleController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rules = await _context.FraudRules.ToListAsync();

            return View(rules);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(FraudRule rule)
        {
            _context.FraudRules.Add(rule);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}