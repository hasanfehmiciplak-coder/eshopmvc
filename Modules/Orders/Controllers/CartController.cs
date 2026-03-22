using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Orders.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        private const string CartSession = "Cart";

        public async Task<IActionResult> Index()
        {
            var items = await _cartService.GetItemsAsync();
            return View(items);
        }

        public IActionResult AddToCart(CartItem item)
        {
            var cart = GetCart();

            var existing = cart.FirstOrDefault(x =>
                x.ProductId == item.ProductId &&
                x.VariantId == item.VariantId);

            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                item.Quantity = 1;
                cart.Add(item);
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        private List<CartItem> GetCart()
        {
            var cart = HttpContext.Session.GetString(CartSession);

            if (cart == null)
                return new List<CartItem>();

            return JsonConvert.DeserializeObject<List<CartItem>>(cart);
        }

        //<<-- -->>
        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(
                CartSession,
                JsonConvert.SerializeObject(cart));
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