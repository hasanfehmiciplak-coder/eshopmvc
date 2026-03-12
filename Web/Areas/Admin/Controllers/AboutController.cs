using EShopMVC.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class AboutController : Controller
{
    private readonly AppDbContext _context;

    public AboutController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var firm = _context.Firms.FirstOrDefault();
        return View(firm);
    }
}