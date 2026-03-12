using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Modules.Fraud.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SecurityController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SecurityController(AppDbContext context,
                                  UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.LockedUsers = await _userManager.Users
                .Where(x => x.LockoutEnd != null && x.LockoutEnd > DateTimeOffset.Now)
                .ToListAsync();

            var bannedIps = await _context.BannedIps
                .Where(x => x.IsActive)
                .ToListAsync();

            return View(bannedIps);
        }

        [HttpPost]
        public async Task<IActionResult> BanIp(string ip, string reason)
        {
            _context.BannedIps.Add(new BannedIp
            {
                IpAddress = ip,
                Reason = reason,
                BannedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UnbanIp(int id)
        {
            var ip = await _context.BannedIps.FindAsync(id);
            if (ip != null)
            {
                ip.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            return RedirectToAction("Index");
        }
    }
}