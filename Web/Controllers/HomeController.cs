using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EShopMVC.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // ============================================
        // 1) **ANA SAYFA (Index) — artýk eksiksiz**
        // ============================================
        public async Task<IActionResult> Index(
                   int page = 1,
                   string? searchString = null,
                   string? sortOrder = null,
                   decimal? minPrice = null,
                   decimal? maxPrice = null)
        {
            // Eðer kullanýcý login ve admin ise Dashboard yönlendirmesi
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
            }

            // Normal kullanýcý veya anonim için ürün listesi
            int pageSize = 8;
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(p => p.Name.Contains(searchString));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            int totalItems = query.Count();

            var products = query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            var model = new ProductListVM
            {
                Products = products,
                CurrentPage = page,
                //TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                SearchString = searchString,
                SortOrder = sortOrder,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            return View(model);
        }

        // ============================================
        // 2) AJAX Ürün Listeleme (Scroll/Paging)
        // ============================================
        public IActionResult ProductList(int page = 1, string? searchString = null,
            string? sortOrder = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            int pageSize = 8;

            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(p => p.Name.Contains(searchString));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            int totalItems = query.Count();

            var products = query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            var model = new ProductListVM
            {
                Products = products,
                CurrentPage = page,
                //TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                SearchString = searchString,
                SortOrder = sortOrder,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            return PartialView("ProductList", model);
        }

        // ============================================
        // 3) Ürün Detaylarý (Modal)
        // ============================================
        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products
                                  .Include(p => p.Category)
                                  .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            return PartialView("_ProductDetailsPartial", product);
        }

        // ============================================
        // 4) Privacy Page
        // ============================================
        public IActionResult Privacy()
        {
            return View();
        }

        // ============================================
        // 5) Error Page
        // ============================================
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}