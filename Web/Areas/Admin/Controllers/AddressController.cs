using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Modules.Orders.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class AddressController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var addresses = _context.Addresses
                .Where(a => a.UserId == user.Id)
                .ToList();

            return View(addresses);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerAddress model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
                return View(model);

            // 🔴 Eğer yeni adres varsayılan seçildiyse
            if (model.IsDefault)
            {
                var oldDefaults = _context.Addresses
                    .Where(x => x.UserId == user.Id && x.IsDefault);

                foreach (var addr in oldDefaults)
                    addr.IsDefault = false;
            }

            model.UserId = user.Id;

            _context.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (address == null)
                return NotFound();

            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerAddress model)
        {
            var user = await _userManager.GetUserAsync(User);

            var address = await _context.Addresses
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.UserId == user.Id);

            if (address == null)
                return NotFound();

            if (model.IsDefault)
            {
                var oldDefaults = _context.Addresses
                    .Where(x => x.UserId == user.Id && x.IsDefault);

                foreach (var addr in oldDefaults)
                    addr.IsDefault = false;
            }

            address.Title = model.Title;
            address.City = model.City;
            address.District = model.District;
            address.Phone = model.Phone;
            address.IsDefault = model.IsDefault;
            address.PostalCode = model.PostalCode;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}