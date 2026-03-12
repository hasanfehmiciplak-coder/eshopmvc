using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Modules.Orders.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.ReportingServices.Interfaces;
using System.Security.Claims;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartService(AppDbContext context, IHttpContextAccessor http, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    private string? UserId =>
        _http.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<int> GetCartItemCountAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User?
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return 0;

        return await _context.Carts
            .Where(c => c.UserId == userId)
            .SelectMany(c => c.CartItems)
            .SumAsync(ci => ci.Quantity);
    }

    public async Task<List<CartItem>> GetItemsAsync()
    {
        if (UserId == null) return new();

        return await _context.CartItems
            .Include(x => x.Product)
            .Where(x => x.Cart.UserId == UserId)
            .ToListAsync();
    }

    public async Task<int> GetItemCountAsync()
    {
        if (UserId == null) return 0;

        return await _context.CartItems
            .Where(x => x.Cart.UserId == UserId)
            .SumAsync(x => x.Quantity);
    }

    public async Task AddAsync(int productId, int quantity = 1)
    {
        if (UserId == null) return;

        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.UserId == UserId);

        if (cart == null)
        {
            cart = new Cart { UserId = UserId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        var item = await _context.CartItems
            .FirstOrDefaultAsync(x =>
                x.CartId == cart.Id &&
                x.ProductId == productId);

        if (item == null)
        {
            _context.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity
            });
        }
        else
        {
            item.Quantity += quantity;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateQuantityAsync(int productId, int quantity)
    {
        if (UserId == null) return;

        var item = await _context.CartItems
            .Include(x => x.Cart)
            .FirstOrDefaultAsync(x =>
                x.ProductId == productId &&
                x.Cart.UserId == UserId);

        if (item == null) return;

        item.Quantity = quantity;
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(int productId)
    {
        if (UserId == null) return;

        var item = await _context.CartItems
            .Include(x => x.Cart)
            .FirstOrDefaultAsync(x =>
                x.ProductId == productId &&
                x.Cart.UserId == UserId);

        if (item == null) return;

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User?
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return new List<CartItem>();

        // 🧠 SENİN YAPINA GÖRE DÜZENLE
        return await _context.CartItems
            .Include(x => x.Product)
            .Where(x => x.Cart.UserId == userId)
            .ToListAsync();
    }

    public async Task ClearCartAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User?
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        var cartItems = await _context.CartItems
            .Where(x => x.Cart.UserId == userId)
            .ToListAsync();

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();
    }

    public async Task ClearAsync()
    {
        var user = await _userManager.GetUserAsync(
            _httpContextAccessor.HttpContext.User);

        if (user == null)
            return;

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (cart == null || !cart.CartItems.Any())
            return;

        _context.CartItems.RemoveRange(cart.CartItems);
        await _context.SaveChangesAsync();
    }
}