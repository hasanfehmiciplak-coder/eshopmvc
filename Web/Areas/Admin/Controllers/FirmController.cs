using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FirmController : Controller
    {
        private readonly AppDbContext _context;

        public FirmController(AppDbContext context)
        {
            _context = context;
        }

        // ------------------- LISTE -------------------
        public async Task<IActionResult> Index()
        {
            var firms = await _context.Firms
                .Where(f => f.IsActive)
                .Include(f => f.Products)
                .ToListAsync();

            return View(firms);  // ✔ Index'e liste gider
        }

        // ------------------- EDIT GET -------------------
        public async Task<IActionResult> Edit(int id)
        {
            var firm = await _context.Firms.FindAsync(id);

            if (firm == null)
                return NotFound();

            return View(firm);  // ❗ TEK MODEL gönderiyoruz
        }

        // ------------------- EDIT POST -------------------
        [HttpPost]
        public async Task<IActionResult> Edit(Firm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Firms.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ------------------- CREATE GET -------------------
        public IActionResult Create()
        {
            return View(new Firm());
        }

        // ------------------- CREATE POST -------------------
        [HttpPost]
        public async Task<IActionResult> Create(Firm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _context.Firms.AddAsync(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var firm = await _context.Firms.FindAsync(id);

            if (firm == null)
                return NotFound();

            firm.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Firma pasif hale getirildi";
            return RedirectToAction(nameof(Index));
        }
    }
}