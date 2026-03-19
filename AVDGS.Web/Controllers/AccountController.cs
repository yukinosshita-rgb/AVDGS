using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

using AVDGS_BLL;
using AVDGS.Web.Models.ViewModels;

namespace AVDGS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _auth;

        public AccountController(AuthService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["HideNav"] = true;

            // ✅ If already authenticated via cookie, go Dashboard
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            ViewData["HideNav"] = true;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var (ok, error, user) = await _auth.ValidateLoginAsync(model.Username, model.Password);

                if (!ok || user == null)
                {
                    model.GeneralError = error ?? "Invalid username or password.";
                    return View(model);
                }

                // ✅ Create cookie identity (this fixes [Authorize] pages)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    });

                // (Optional) keep session if your UI uses it
                HttpContext.Session.SetString("LoggedInUser", user.Username);
                HttpContext.Session.SetString("LoggedInUserId", user.UserId.ToString());

                // ✅ Safe returnUrl redirect
                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                model.GeneralError = "System error: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
