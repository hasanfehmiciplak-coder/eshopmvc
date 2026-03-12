using Microsoft.AspNetCore.Mvc;

public class CartBadgeViewComponent : ViewComponent
{
    private readonly ICartService _cartService;

    public CartBadgeViewComponent(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = await _cartService.GetCartItemCountAsync();
        return View(count);
    }
}