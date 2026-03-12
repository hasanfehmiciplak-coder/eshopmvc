using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Firm)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(
                _context.Categories.Where(c => c.IsActive),
                "Id", "Name");

            ViewBag.Firms = new SelectList(
                _context.Firms.Where(f => f.IsActive),
                "Id", "Name");
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"MODEL ERROR [{state.Key}]: {error.ErrorMessage}");
                    }
                }

                LoadDropdowns(product);
                return View(product);
            }

            if (product.CategoryId == 0)
                ModelState.AddModelError("CategoryId", "Kategori seçmelisiniz.");

            var firm = await _context.Firms.FindAsync(product.FirmId);
            if (firm == null || !firm.IsActive)
                ModelState.AddModelError("FirmId", "Pasif veya geçersiz firmaya ürün eklenemez.");

            if (!ModelState.IsValid)
            {
                LoadDropdowns(product);
                return View(product);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(uploads, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                product.ImageUrl = "/uploads/" + fileName;
            }

            product.Slug = product.Name
                .ToLower()
                .Replace(" ", "-")
                .Replace("ç", "c")
                .Replace("ğ", "g")
                .Replace("ı", "i")
                .Replace("ö", "o")
                .Replace("ş", "s")
                .Replace("ü", "u");

            var testProduct = new Product
            {
                Name = "TEST ÜRÜN",
                Price = 10,
                Stock = 1,
                CategoryId = product.CategoryId,
                FirmId = product.FirmId,
                Slug = "test-urun"
            };

            _context.Products.Add(product);
            Console.WriteLine("BEFORE SAVE");
            await _context.SaveChangesAsync();
            Console.WriteLine("AFTER SAVE, ID = " + product.Id);
            var count = await _context.Products.CountAsync();
            Console.WriteLine("TOTAL PRODUCT COUNT: " + count);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(
                _context.Categories.Where(x => x.IsActive),
                "Id", "Name", product.CategoryId);

            ViewBag.Firms = new SelectList(
                _context.Firms.Where(x => x.IsActive),
                "Id", "Name", product.FirmId);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return BadRequest();

            // 🔒 Kategori zorunlu
            if (product.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Kategori seçmelisiniz.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    _context.Categories.Where(x => x.IsActive),
                    "Id", "Name",
                    product.CategoryId);

                ViewBag.Firms = new SelectList(
                    _context.Firms.Where(x => x.IsActive),
                    "Id", "Name",
                    product.FirmId);

                return View(product);
            }

            // 🔒 Firma aktif mi?
            var firm = await _context.Firms.FindAsync(product.FirmId);
            if (firm == null || !firm.IsActive)
            {
                ModelState.AddModelError("", "Pasif firmaya ürün bağlanamaz.");

                ViewBag.Categories = new SelectList(
                    _context.Categories.Where(x => x.IsActive),
                    "Id", "Name",
                    product.CategoryId);

                ViewBag.Firms = new SelectList(
                    _context.Firms.Where(x => x.IsActive),
                    "Id", "Name",
                    product.FirmId);

                return View(product);
            }

            // 🖼️ Yeni resim geldiyse değiştir
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(uploads, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                product.ImageUrl = "/uploads/" + fileName;
            }
            product.Slug = product.Name
                .ToLower()
                .Replace(" ", "-")
                .Replace("ç", "c")
                .Replace("ğ", "g")
                .Replace("ı", "i")
                .Replace("ö", "o")
                .Replace("ş", "s")
                .Replace("ü", "u");

            _context.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            // 🔥 Fotoğrafı sil
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var filePath = Path.Combine(
                    _env.WebRootPath,
                    product.ImageUrl.TrimStart('/')
                );

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ürün silindi";
            return RedirectToAction(nameof(Index));
        }

        //[HttpPost]
        //public async Task<IActionResult> ToggleStatus(int id)
        //{
        //    var product = await _context.Products.FindAsync(id);
        //    if (product == null)
        //        return Json(new { success = false, message = "Ürün bulunamadı" });

        //    product.IsActive = !product.IsActive;
        //    await _context.SaveChangesAsync();

        //    return Json(new
        //    {
        //        success = true,
        //        status = product.IsActive ? "Aktif" : "Pasif"
        //    });
        //}

        public IActionResult FilterByCategory(int categoryId)
        {
            var products = _context.Products
                .Include(p => p.Firm)
                .Where(p => p.IsActive && p.Firm.IsActive)
                .AsQueryable();

            if (categoryId != 0)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            return PartialView("_ProductListPartial", products.ToList());
        }

        public async Task<IActionResult> DetailPartial(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Firm)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.IsActive &&
                    p.Firm.IsActive);

            if (product == null)
                return NotFound();

            return PartialView("_ProductDetailPartial", product);
        }

        //private void LoadDropdowns(Product? product = null)
        //{
        //    ViewBag.Categories = _context.Categories
        //        .Where(c => c.IsActive)
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.Id.ToString(),
        //            Text = c.Name,
        //            Selected = product != null && c.Id == product.CategoryId
        //        })
        //        .ToList();

        //    ViewBag.Firms = _context.Firms
        //        .Where(f => f.IsActive)
        //        .Select(f => new SelectListItem
        //        {
        //            Value = f.Id.ToString(),
        //            Text = f.Name,
        //            Selected = product != null && f.Id == product.FirmId
        //        })
        //        .ToList();
        //}

        private void LoadFirms()
        {
            ViewBag.Firms = _context.Firms
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = f.Name
                })
                .ToList();
        }

        private void LoadDropdowns(Product? product = null)
        {
            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = product != null && c.Id == product.CategoryId
                })
                .ToList();

            ViewBag.Firms = _context.Firms
                .Where(f => f.IsActive)
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = f.Name,
                    Selected = product != null && f.Id == product.FirmId
                })
                .ToList();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.IsActive = !product.IsActive;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = product.IsActive
            });
        }
    }
}