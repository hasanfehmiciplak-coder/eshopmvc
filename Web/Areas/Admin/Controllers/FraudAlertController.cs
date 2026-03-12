using EShopMVC.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FraudAlertController : Controller
    {
        private readonly AppDbContext _context;

        public FraudAlertController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var alerts = await _context.FraudAlerts
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(alerts);
        }
    }
}