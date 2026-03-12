//using EShopMVC.Data;
//using EShopMVC.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace EShopMVC.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    public class AdminController : Controller
//    {
//        private readonly AppDbContext _context;

//        public AdminController(AppDbContext context)
//        {
//            _context = context;
//        }

//        // Ürün listesi
//        public IActionResult Products()
//        {
//            var products = _context.Products.ToList();
//            return View(products);
//        }

//        // Yeni ürün ekleme
//        [HttpGet]
//        public IActionResult CreateProduct()
//        {
//            return View();
//        }

//        [HttpPost]
//        public IActionResult CreateProduct(Product product)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Products.Add(product);
//                _context.SaveChanges();
//                return RedirectToAction("Products");
//            }
//            return View(product);
//        }

//        // Ürün düzenleme
//        [HttpGet]
//        public IActionResult EditProduct(int id)
//        {
//            var product = _context.Products.FirstOrDefault(p => p.Id == id);
//            if (product == null) return NotFound();
//            return View(product);
//        }

//        [HttpPost]
//        public IActionResult EditProduct(Product product)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Products.Update(product);
//                _context.SaveChanges();
//                return RedirectToAction("Products");
//            }
//            return View(product);
//        }

//        // Ürün silme
//        [HttpPost]
//        public IActionResult DeleteProduct(int id)
//        {
//            var product = _context.Products.FirstOrDefault(p => p.Id == id);
//            if (product != null)
//            {
//                _context.Products.Remove(product);
//                _context.SaveChanges();
//            }
//            return RedirectToAction("Products");
//        }

//        public IActionResult Categories()
//        {
//            var categories = _context.Categories.ToList();
//            return View(categories);
//        }

//        [HttpGet]
//        public IActionResult CreateCategory()
//        {
//            return View();
//        }

//        [HttpPost]
//        public IActionResult CreateCategory(Category category)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Categories.Add(category);
//                _context.SaveChanges();
//                return RedirectToAction("Categories");
//            }
//            return View(category);
//        }

//        [HttpGet]
//        public IActionResult EditCategory(int id)
//        {
//            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
//            if (category == null) return NotFound();
//            return View(category);
//        }

//        [HttpPost]
//        public IActionResult EditCategory(Category category)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Categories.Update(category);
//                _context.SaveChanges();
//                return RedirectToAction("Categories");
//            }
//            return View(category);
//        }

//        [HttpPost]
//        public IActionResult DeleteCategory(int id)
//        {
//            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
//            if (category != null)
//            {
//                _context.Categories.Remove(category);
//                _context.SaveChanges();
//            }
//            return RedirectToAction("Categories");
//        }

//        // Sipariş listesi
//        public IActionResult Orders()
//        {
//            var orders = _context.Orders
//                                 .Include(o => o.Items)
//                                 .ThenInclude(i => i.Product)
//                                 .OrderByDescending(o => o.OrderDate)
//                                 .ToList();

//            return View(orders);
//        }

//        // Sipariş detayları
//        public IActionResult OrderDetails(int id)
//        {
//            var order = _context.Orders
//                .Include(o => o.Items)
//                .ThenInclude(i => i.Product)
//                .FirstOrDefault(o => o.Id == id);

//            if (order == null) return NotFound();

//            return PartialView("_OrderDetailsPartial", order);
//        }

//        [HttpPost]
//        public IActionResult UpdateOrderStatus(int id, OrderStatus status)
//        {
//            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
//            if (order == null) return Json(new { success = false });

//            order.Status = status;
//            _context.SaveChanges();

//            // Türkçe metin ve badge class
//            string statusText = status switch
//            {
//                OrderStatus.Pending => "Beklemede",
//                OrderStatus.Shipped => "Kargoya Verildi",
//                OrderStatus.Completed => "Teslim Edildi",
//                OrderStatus.Approved => "Onaylandı",
//                OrderStatus.Rejected => "Reddedildi",
//                _ => status.ToString()
//            };

//            string badgeClass = status switch
//            {
//                OrderStatus.Pending => "bg-warning text-dark",
//                OrderStatus.Shipped => "bg-info text-dark",
//                OrderStatus.Completed => "bg-success",
//                OrderStatus.Approved => "bg-primary",
//                OrderStatus.Rejected => "bg-danger",
//                _ => "bg-secondary"
//            };

//            return Json(new { success = true, newStatusText = statusText, newStatusClass = badgeClass });
//        }

//        public IActionResult Orders(OrderStatus? status, string search)
//        {
//            var query = _context.Orders
//                                .Include(o => o.Items)
//                                .ThenInclude(i => i.Product)
//                                .AsQueryable();

//            if (status.HasValue)
//            {
//                query = query.Where(o => o.Status == status.Value);
//            }

//            if (!string.IsNullOrEmpty(search))
//            {
//                query = query.Where(o => o.CustomerName.Contains(search));
//            }

//            var orders = query.OrderByDescending(o => o.OrderDate).ToList();
//            return View(orders);
//        }
//    }
//}