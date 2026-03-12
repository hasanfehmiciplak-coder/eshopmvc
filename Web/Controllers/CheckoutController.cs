using DocumentFormat.OpenXml.Spreadsheet;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Modules.Orders.Models;
using EShopMVC.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class CheckoutController : Controller
{
    private readonly AppDbContext _context;
    private readonly ICartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CheckoutController(
        AppDbContext context,
        ICartService cartService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _cartService = cartService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var cartItems = await _cartService.GetItemsAsync();

        if (cartItems == null || !cartItems.Any())
            return RedirectToAction("Index", "Cart");

        return View(new CheckoutVM());
    }

    [HttpPost]
    public async Task<IActionResult> Index(CheckoutVM model)
    {
        Console.WriteLine("🔥 CHECKOUT POST ACTION HIT 🔥");

        // ✅ SADECE BURADA AL
        var user = await _userManager.GetUserAsync(User);

        // 🔒 Login user → Email validation kaldır
        if (user != null)
        {
            ModelState.Remove(nameof(model.Email));
        }

        if (!ModelState.IsValid)
        {
            Console.WriteLine("MODEL STATE INVALID");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return View(model);
        }

        var cartItems = await _cartService.GetItemsAsync();

        if (cartItems == null || !cartItems.Any())
            return RedirectToAction("Index", "Cart");

        var totalPrice = cartItems.Sum(x => x.Product.Price * x.Quantity);

        // 🔒 Stok tekrar kontrol
        foreach (var item in cartItems)
        {
            var stock = await _context.Products
                .Where(p => p.Id == item.ProductId)
                .Select(p => p.Stock)
                .FirstAsync();

            if (stock < item.Quantity)
            {
                ModelState.AddModelError("", "Stok yetersiz.");
                return View(model);
            }
        }

        var order = new Order
        {
            UserId = user?.Id,

            // 🔴 DB SNAPSHOT ALANLARI (NOT NULL)
            CustomerEmail = user?.Email ?? model.Email,
            CustomerName = user != null
                ? user.UserName
                : model.FullName,

            // 🔥 BU SATIR ŞART
            FullName = user != null
        ? user.UserName
        : model.FullName,

            Phone = !string.IsNullOrWhiteSpace(model.Phone)
                ? model.Phone
                : user?.PhoneNumber ?? "0000000000",

            Address = model.Address,
            TotalPrice = totalPrice,
            OrderDate = DateTime.UtcNow
        };

        // 🔥 KORUYUCU GUARD
        if (string.IsNullOrWhiteSpace(order.CustomerEmail) ||
            string.IsNullOrWhiteSpace(order.FullName) ||
            string.IsNullOrWhiteSpace(order.Phone))
        {
            throw new InvalidOperationException(
                $"Order create failed. Email='{order.CustomerEmail}', FullName='{order.FullName}', Phone='{order.Phone}'"
            );
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 📦 ORDER ITEMS
        foreach (var item in cartItems)
        {
            _context.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Price = item.Product.Price,
                Quantity = item.Quantity
            });

            item.Product.Stock -= item.Quantity;
        }

        await _context.SaveChangesAsync();

        // 🧹 DB sepetini temizle
        await _cartService.ClearAsync();

        return RedirectToAction("Success", new { id = order.Id });
    }

    public IActionResult Success(int id)
    {
        return RedirectToAction(
            "StartPayment",
            "Payment",
            new { orderId = id }
        );
    }

    [Authorize]
    public async Task<IActionResult> Checkout()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        if (!user.EmailConfirmed)
        {
            TempData["Error"] =
                "Ödeme yapabilmek için e-posta adresinizi doğrulamanız gerekiyor.";

            return RedirectToAction("EmailNotConfirmed", "Account");
        }

        // ✅ Email doğrulanmış → checkout sayfası
        return View();
    }

    // 📄 GET → Adres Seçimi
    //public async Task<IActionResult> Index()
    //{
    //    var cart = _cartService.GetCart();
    //    if (!cart.Items.Any())
    //        return RedirectToAction("Index", "Cart");

    //    var user = await _userManager.GetUserAsync(User);

    //    var addresses = await _context.Addresses
    //        .Where(a => a.UserId == user.Id)
    //        .ToListAsync();

    //    ViewBag.Addresses = addresses;
    //    return View(cart);
    //}

    // ✅ POST → Siparişi Tamamla
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Complete(int addressId)
    //{
    //    var cart = _cartService.GetCart();
    //    if (!cart.Items.Any())
    //        return RedirectToAction("Index", "Cart");

    //    var user = await _userManager.GetUserAsync(User);

    //    var order = new Order
    //    {
    //        UserId = user.Id,
    //        AddressId = addressId,
    //        TotalAmount = cart.TotalPrice,
    //        Status = OrderStatus.Beklemede,
    //        OrderDate = DateTime.Now
    //    };

    //    foreach (var item in cart.Items)
    //    {
    //        order.OrderItems.Add(new OrderItem
    //        {
    //            ProductId = item.ProductId,
    //            Price = item.Price,
    //            Quantity = item.Quantity
    //        });
    //    }

    //    _context.Orders.Add(order);
    //    await _context.SaveChangesAsync();

    //    // 🧹 Sepeti temizle
    //    _cartService.RemoveAll();

    //    return RedirectToAction("MyOrders", "Orders");
    //}
}