using EShopMVC.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Web.Controllers
{
    [Authorize(Roles = "User")]
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }

        // Tüm ürünler
        public IActionResult Index()
        {
            var products = _context.Products
                .Where(p => p.IsActive)
                .ToList();
            return View(products);
        }

        // Kategoriye göre ürünler
        public IActionResult ByCategory(int id)
        {
            var products = _context.Products
                                   .Include(p => p.Category)
                                   .Where(p => p.CategoryId == id)
                                   .ToList();

            ViewBag.CategoryName = _context.Categories.FirstOrDefault(c => c.Id == id)?.Name;
            return View("Index", products); // aynı view kullanılıyor
        }

        // AJAX için kategoriye göre ürünler
        public IActionResult ProductsByCategory(int id)
        {
            var products = _context.Products
                                   .Include(p => p.Category)
                                   .Where(p => p.CategoryId == id)
                                   .ToList();

            return PartialView("_ProductListPartial", products);
        }
    }
}