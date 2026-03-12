//using Microsoft.AspNetCore.Mvc;
//using EShopMVC.Data;
//using Microsoft.AspNetCore.Authorization;

//[Authorize(Roles = "User")] // sadece kullanıcı rolü erişebilir
//public class MyOrdersController : Controller
//{
//    private readonly AppDbContext _context;

//    public MyOrdersController(AppDbContext context)
//    {
//        _context = context;
//    }

//    public IActionResult Index()
//    {
//        // Burada kullanıcı kimliğine göre filtreleme yapılmalı
//        var userName = User.Identity?.Name;
//        var orders = _context.Orders
//            .Where(o => o.CustomerName == userName)
//            .ToList();

//        return View(orders);
//    }

//    public IActionResult Details(int id)
//    {
//        var order = _context.Orders.Find(id);
//        if (order == null) return NotFound();

//        // Kullanıcı sadece kendi siparişini görebilmeli
//        if (order.CustomerName != User.Identity?.Name)
//            return Unauthorized();

//        return View(order);
//    }
//}