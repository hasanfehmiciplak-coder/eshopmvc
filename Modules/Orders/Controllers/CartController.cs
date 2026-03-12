using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EShopMVC.Modules.Orders.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;
        private readonly AppDbContext _context;
        private const string CartKey = "CART";

        public CartController(ICartService cartService, ILogger<CartController> logger, AppDbContext context)
        {
            _cartService = cartService;
            _logger = logger;
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            Console.WriteLine(
                "SESSION ID (Cart): " + HttpContext.Session.Id);

            await _cartService.AddAsync(productId, quantity);

            var count = await _cartService.GetItemCountAsync();

            return Json(new { success = true, count });
        }

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _cartService.GetItemCountAsync();
            return Json(count);
        }

        public async Task<IActionResult> Index()
        {
            var items = await _cartService.GetItemsAsync();
            return View(items);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1)
                return Json(new { success = false });

            await _cartService.UpdateQuantityAsync(productId, quantity);

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            await _cartService.RemoveAsync(productId);
            return Json(new { success = true });
        }
    }
}