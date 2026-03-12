using EShopMVC.Models;
using EShopMVC.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _env = env;
    }

    // GET: /Profile
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var vm = new ProfileViewModel
        {
            FullName = user.FullName,
            AvatarPath = user.AvatarPath
        };

        return View(vm);
    }

    // POST: /Profile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ApplicationUser model, IFormFile? avatarFile)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;

        // 🔵 AVATAR UPLOAD
        if (avatarFile != null && avatarFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await avatarFile.CopyToAsync(stream);

            user.AvatarPath = "/uploads/" + fileName;
        }

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            ViewBag.Success = "Profil güncellendi ✔️";
        else
            ViewBag.Error = "Profil güncellenemedi ❌";

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Index));

        var user = await _userManager.GetUserAsync(User);

        user.FullName = model.FullName;

        // 🔹 Avatar upload
        if (model.AvatarFile != null)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads/avatars");
            Directory.CreateDirectory(uploads);

            var fileName = $"{user.Id}_{Path.GetExtension(model.AvatarFile.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await model.AvatarFile.CopyToAsync(stream);

            user.AvatarPath = "/uploads/avatars/" + fileName;
        }

        await _userManager.UpdateAsync(user);

        TempData["Success"] = "Profil güncellendi";
        return RedirectToAction(nameof(Index));
    }
}