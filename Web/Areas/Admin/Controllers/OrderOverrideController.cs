using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderOverrideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderOverrideController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> EnableRefundOverride(
            int orderId,
            string note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return BadRequest("Override açıklaması zorunludur");

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            order.RefundOverrideEnabled = true;
            order.RefundOverrideNote = note;
            order.RefundOverrideAt = DateTime.Now;
            order.RefundOverrideByUserId = _userManager.GetUserId(User);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}