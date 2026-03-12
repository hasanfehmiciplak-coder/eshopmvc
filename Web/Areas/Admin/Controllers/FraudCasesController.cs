using EShopMVC.Hubs;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FraudCasesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<FraudAlertHub> _hub;

        public FraudCasesController(AppDbContext context, IHubContext<FraudAlertHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task<IActionResult> Create(int orderId)
        {
            var exists = await _context.FraudCases
                .AnyAsync(x => x.OrderId == orderId && x.Status == Modules.Fraud.Models.FraudCaseStatus.Open);

            if (!exists)
            {
                _context.FraudCases.Add(new FraudCase
                {
                    OrderId = orderId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(
                "Investigation",
                "Fraud",
                new { orderId });
        }

        public async Task<IActionResult> Close(int id)
        {
            var fraudCase = await _context.FraudCases.FindAsync(id);

            if (fraudCase == null)
                return NotFound();

            fraudCase.Status = Modules.Fraud.Models.FraudCaseStatus.Closed;
            fraudCase.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> Details(int id)
        {
            var fraudCase = await _context.FraudCases
                .FirstOrDefaultAsync(x => x.Id == id);

            if (fraudCase == null)
                return NotFound();

            var timeline = await _context.OrderTimelines
                .Where(x => x.OrderId == fraudCase.OrderId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            ViewBag.Timeline = timeline;

            return View(fraudCase);
        }

        [HttpPost]
        public async Task<IActionResult> AddNote(int caseId, string note)
        {
            var fraudCase = await _context.FraudCases
                .FirstOrDefaultAsync(x => x.Id == caseId);

            if (fraudCase == null)
                return NotFound();

            fraudCase.Notes = note;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = caseId });
        }
    }
}