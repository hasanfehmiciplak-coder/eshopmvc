using EShopMVC.Models;
using EShopMVC.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EShopMVC.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly ICartService _cartService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            ICartService cartService)// 👈 SADECE BU
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _cartService = cartService;
        }

        // ===============================
        // GET LOGIN
        // ===============================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        // ===============================
        // POST LOGIN
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Geçersiz bilgi" });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Json(new { success = false, message = "Kullanıcı bulunamadı" });

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true
            );

            if (result.IsNotAllowed)
                return Json(new { success = false, message = "E-posta doğrulanmamış" });

            if (result.IsLockedOut)
                return Json(new { success = false, message = "Hesap kilitli" });

            if (!result.Succeeded)
                return Json(new { success = false, message = "E-posta veya şifre hatalı" });

            // ✅ BAŞARILI GİRİŞ

            var roles = await _userManager.GetRolesAsync(user);

            string redirectUrl;

            // 🔑 ReturnUrl öncelikli
            if (!string.IsNullOrEmpty(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                redirectUrl = model.ReturnUrl;
            }
            else
            {
                redirectUrl = roles.Contains("Admin")
                    ? "/Admin/Dashboard"
                    : "/";
            }

            return Json(new { success = true, redirectUrl });
        }

        // ===============================
        // REGISTER (POST)
        // ===============================
        // KAYIT SUBMIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return Json(new
                {
                    success = false,
                    message = "Form geçersiz",
                    errors
                });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true // 🔥 DEV İÇİN KRİTİK
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return Json(new { success = false, message = "Kayıt başarısız" });
            }

            await _userManager.AddToRoleAsync(user, "User");

            return Json(new
            {
                success = true,
                message = "Kayıt başarılı."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }

        public IActionResult Profile()
        {
            return View();
        }

        // ===============================
        // NAVBAR PARTIAL (AJAX)
        // ===============================
        [HttpGet]
        public IActionResult GetNavbar()
        {
            return PartialView("_NavbarPartial");
        }

        // KAYIT FORMU
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false });

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            return Json(new
            {
                success = true,
                status = user.IsActive ? "Aktif" : "Pasif"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(string email, string password, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !user.IsActive)
                return Json(new { success = false, message = "Yetkisiz giriş." });

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                return Json(new { success = false, message = "Admin yetkiniz yok." });

            var result = await _signInManager.PasswordSignInAsync(
                user, password, rememberMe, false);

            if (!result.Succeeded)
                return Json(new { success = false, message = "Hatalı giriş." });

            return Json(new
            {
                success = true,
                redirectUrl = "/Admin/Dashboard"
            });
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private JsonResult Error(string message)
        {
            return Json(new { success = false, message });
        }

        private JsonResult Success(string redirectUrl)
        {
            return Json(new { success = true, redirectUrl });
        }

        private async Task SendEmailConfirmation(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, token },
                Request.Scheme
            );

            //    await _emailSender.SendAsync(
            //        user.Email,
            //        "Email Doğrulama",
            //        $"""
            //Merhaba {user.FullName},

            //Hesabınızı doğrulamak için aşağıdaki linke tıklayın:
            //{link}
            //"""
            //    );
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("forgot")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Error("Email adresi zorunludur.");

            var user = await _userManager.FindByEmailAsync(email);

            // 🔒 Güvenlik: kullanıcı var/yok fark etmeksizin aynı cevap
            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var link = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { email, token },
                    Request.Scheme
                );

                //                await _emailSender.SendAsync(
                //                    email,
                //                    "Şifre Sıfırlama",
                //                 $"""
                //Merhaba,<br/><br/>

                //Şifrenizi sıfırlamak için aşağıdaki linke tıklayın:<br/>
                //<a href="{link}">Şifreyi Sıfırla</a><br/><br/>

                //Eğer bu isteği siz yapmadıysanız bu maili yok sayabilirsiniz.
                //"""
                //                );
            }

            return Json(new
            {
                success = true,
                message = "Eğer bu email adresi sistemimizde kayıtlıysa, şifre sıfırlama linki gönderildi."
            });
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Error");

            return View(new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return Error("Form geçersiz.");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Error("Geçersiz istek.");

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password
            );

            if (!result.Succeeded)
            {
                return Error(string.Join(" | ",
                    result.Errors.Select(e => e.Description)));
            }

            return Json(new
            {
                success = true,
                message = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz."
            });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return View("ConfirmEmailFailed");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return View("ConfirmEmailFailed");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                return View("ConfirmEmailFailed");

            return View("ConfirmEmailSuccess");
        }

        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View(new ResendEmailConfirmationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(
    ResendEmailConfirmationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Güvenlik: Kullanıcı yoksa da aynı mesaj
            if (user == null)
            {
                ViewBag.Message =
                    "Eğer bu e-posta sistemde kayıtlıysa, doğrulama maili gönderildi.";
                return View("ResendEmailConfirmationResult");
            }

            if (user.EmailConfirmed)
            {
                ViewBag.Message = "Bu e-posta zaten doğrulanmış.";
                return View("ResendEmailConfirmationResult");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmLink = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, token },
                protocol: Request.Scheme
            );

            // DEV MODE
            _logger.LogInformation("RESEND CONFIRM LINK: {Link}", confirmLink);

            ViewBag.Message =
                "Doğrulama e-postası yeniden gönderildi. Lütfen e-postanızı kontrol edin.";

            return View("ResendEmailConfirmationResult");
        }

        [Authorize]
        public IActionResult EmailNotConfirmed()
        {
            return View();
        }
    }
}