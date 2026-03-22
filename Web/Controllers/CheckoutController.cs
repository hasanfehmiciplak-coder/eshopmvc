using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Modules.Orders.Application;
using EShopMVC.Modules.Orders.Domain;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;

public class CheckoutController : Controller
{
    private readonly AppDbContext _context;
    private readonly ICartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CheckoutService _checkoutService;

    public CheckoutController(
        AppDbContext context,
        ICartService cartService,
        UserManager<ApplicationUser> userManager,
        CheckoutService checkoutService)
    {
        _context = context;
        _cartService = cartService;
        _userManager = userManager;
        _checkoutService = checkoutService;
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
        var user = await _userManager.GetUserAsync(User);

        if (!ModelState.IsValid)
            return View(model);

        var cartItems = await _cartService.GetItemsAsync();

        if (cartItems == null || !cartItems.Any())
            return RedirectToAction("Index", "Cart");

        var totalPrice = cartItems.Sum(x => x.Product.UnitPrice * x.Quantity);

        // 🔒 stok kontrol
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

        // ✅ DOĞRU ORDER
        var order = new Order(user.Id, totalPrice);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 📦 ITEMS
        foreach (var item in cartItems)
        {
            order.AddItem(
                item.ProductId,
                item.Quantity,
                item.Product.UnitPrice
            );

            item.Product.Stock -= item.Quantity;
        }

        await _context.SaveChangesAsync();

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
}