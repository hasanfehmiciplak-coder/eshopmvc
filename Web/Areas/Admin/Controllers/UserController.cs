using EShopMVC.Models;
using EShopMVC.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            // Önce kullanıcı listesini çekiyoruz
            var users = await _userManager.Users.ToListAsync();

            var model = new List<UserListViewModel>();

            foreach (var user in users)
            {
                // Burada her kullanıcı için ayrı sorgu yapıyoruz
                // DataReader çakışmasını önlemek için await ile sırayla çalıştırıyoruz
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "User"
                });
            }

            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction("Details", new { id = userId });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { success = false });

            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);
            await _userManager.AddToRoleAsync(user, newRole);

            return Json(new { success = true });
        }

        // 🔒 Aktif / Pasif
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == id)
            {
                return Json(new
                {
                    success = false,
                    message = "Kendi hesabınızı pasif edemezsiniz!"
                });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Kullanıcı bulunamadı" });

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            return Json(new
            {
                success = true,
                isActive = user.IsActive
            });
        }

        // 🎭 Rol Değiştir
        [HttpPost]
        public async Task<IActionResult> ToggleRole(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            // 🔒 Admin kendini kilitlemesin
            if (currentUserId == id)
            {
                return Json(new
                {
                    success = false,
                    message = "Kendi rolünüzü değiştiremezsiniz!"
                });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Kullanıcı bulunamadı" });

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            var newRole = await _userManager.IsInRoleAsync(user, "Admin")
                ? "Admin"
                : "User";

            return Json(new { success = true, role = newRole });
        }
    }
}