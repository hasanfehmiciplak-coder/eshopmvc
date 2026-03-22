using EShopMVC.Infrastructure.Data;
using EShopMVC.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ProductController : Controller
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    // 🟢 USER ÜRÜN LİSTESİ
    public IActionResult Index(ProductListVM vm)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Category.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(vm.Search))
        {
            query = query.Where(p =>
                p.Name.Contains(vm.Search) ||
                p.Description.Contains(vm.Search));
        }

        if (vm.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == vm.CategoryId);

        if (vm.MinPrice.HasValue)
            query = query.Where(p => p.UnitPrice >= vm.MinPrice);

        if (vm.MaxPrice.HasValue)
            query = query.Where(p => p.UnitPrice <= vm.MaxPrice);

        if (vm.InStock)
            query = query.Where(p => p.Stock > 0);

        // 🔢 Toplam kayıt (pagination için)
        vm.TotalCount = query.Count();

        // 🔃 SIRALAMA
        query = vm.Sort switch
        {
            "price_asc" => query.OrderBy(p => p.UnitPrice),
            "price_desc" => query.OrderByDescending(p => p.UnitPrice),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedDate) // newest (default)
        };

        // 🔢 Sayfalı veri
        vm.Products = query
            .OrderByDescending(p => p.CreatedDate)
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToList();

        vm.Categories = _context.Categories
            .Where(c => c.IsActive)
            .ToList();

        return View(vm);
    }

    public IActionResult DetailModal(int id)
    {
        var product = _context.Products
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Id == id && p.IsActive);

        if (product == null)
            return NotFound();

        return PartialView("_ProductDetailModal", product);
    }

    public async Task<IActionResult> DetailPartial(int id)
    {
        var product = await _context.Products
            .Include(p => p.Firm)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
            return NotFound();

        return PartialView("_ProductDetailsPartial", product);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return View(product);
    }

    // 🔍 Ürün Detayı
    //public async Task<IActionResult> Detail(int id)
    //{
    //    var product = await _context.Products
    //        .Include(p => p.Category)
    //         .Include(p => p.Images)
    //        .FirstOrDefaultAsync(p =>
    //            p.Id == id &&
    //            p.IsActive &&
    //            p.Category.IsActive); // 🔥 EKLENDİ

    //    if (product == null)
    //        return NotFound();

    //    var related = await _context.Products
    //        .Include(p => p.Category) // 🔥 EKLENDİ (ÇOK ÖNEMLİ)
    //        .Where(p =>
    //            p.IsActive &&
    //            p.Category.IsActive && // 🔥 EKLENDİ
    //            p.Stock > 0 &&
    //            p.CategoryId == product.CategoryId &&
    //            p.Id != product.Id)
    //        .OrderByDescending(p => p.Id)
    //        .Take(4)
    //        .ToListAsync();

    //    ViewBag.RelatedProducts = related;

    //    return View(product);
    //}

    //[Route("kategori/{slug}")]
    public IActionResult ByCategory(string slug)
    {
        var category = _context.Categories
            .FirstOrDefault(c => c.Slug == slug && c.IsActive);

        if (category == null)
            return NotFound();

        var products = _context.Products
            .Include(p => p.Category)
            .Where(p =>
                p.IsActive &&
                p.Category.IsActive &&
                p.CategoryId == category.Id)
            .OrderByDescending(p => p.CreatedDate)
            .ToList();

        var vm = new ProductListVM
        {
            CategoryId = category.Id,
            Products = products,
            Categories = _context.Categories.Where(c => c.IsActive).ToList()
        };

        ViewBag.CategoryName = category.Name;

        return View("Index", vm); // 🔥 Aynı view kullanılıyor
    }

    [Route("urun/{slug}-{id}")]
    public async Task<IActionResult> SeoDetail(string slug, int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
             .Include(p => p.Images)
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.IsActive &&
                p.Category.IsActive);

        if (product == null)
            return NotFound();

        // ❗ Yanlış slug gelirse doğru URL’ye yönlendir
        if (product.Slug != slug)
        {
            return RedirectToActionPermanent(
                "SeoDetail",
                new { slug = product.Slug, id = product.Id });
        }

        return View("Detail", product); // 🔥 mevcut view
    }
}