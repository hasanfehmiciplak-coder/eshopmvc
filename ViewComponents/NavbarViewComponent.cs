using Microsoft.AspNetCore.Mvc;

public class NavbarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var model = new NavbarViewModel
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            UserName = User.Identity?.Name,
            IsAdmin = User.IsInRole("Admin")
        };

        return View(model);
    }
}