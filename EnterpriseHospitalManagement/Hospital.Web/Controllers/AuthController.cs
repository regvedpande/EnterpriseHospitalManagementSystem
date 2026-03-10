using System.ComponentModel.DataAnnotations;
using Hospital.Models;
using Hospital.Models.Enums;
using Hospital.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly UserManager<ApplicationUser>  _users;

        public AuthController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users)
        {
            _signIn = signIn;
            _users  = users;
        }

        // ── LOGIN ─────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RoleRedirect();
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _users.FindByEmailAsync(model.Email);
            if (user == null) { ModelState.AddModelError("", "Invalid email or password."); return View(model); }

            var result = await _signIn.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
            if (!result.Succeeded) { ModelState.AddModelError("", "Invalid email or password."); return View(model); }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return await RoleRedirectAsync(user);
        }

        // ── REGISTER ──────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Register() => User.Identity?.IsAuthenticated == true ? RoleRedirect() : View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName       = model.Email,
                Email          = model.Email,
                Name           = model.Name,
                Gender         = model.Gender,
                Address        = model.Address ?? "",
                DOB            = model.DOB,
                IsDoctor       = false,
                EmailConfirmed = true
            };

            var result = await _users.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(model);
            }

            await _users.AddToRoleAsync(user, WebSiteRoles.Website_Patient);
            await _signIn.SignInAsync(user, false);
            TempData["success"] = "Welcome! Your account has been created.";
            return RedirectToAction("Index", "Home", new { area = "Patient" });
        }

        // ── LOGOUT ────────────────────────────────────────────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ── ACCESS DENIED ─────────────────────────────────────────────────────

        public IActionResult AccessDenied() => View();

        // ── HELPERS ───────────────────────────────────────────────────────────

        private IActionResult RoleRedirect()
        {
            if (User.IsInRole(WebSiteRoles.Website_Admin))   return RedirectToAction("Index", "Hospitals",  new { area = "Admin" });
            if (User.IsInRole(WebSiteRoles.Website_Doctor))  return RedirectToAction("Index", "Doctors",    new { area = "Doctor" });
            if (User.IsInRole(WebSiteRoles.Website_Patient)) return RedirectToAction("Index", "Home",       new { area = "Patient" });
            return RedirectToAction("Index", "Home");
        }

        private async Task<IActionResult> RoleRedirectAsync(ApplicationUser user)
        {
            var roles = await _users.GetRolesAsync(user);
            if (roles.Contains(WebSiteRoles.Website_Admin))   return RedirectToAction("Index", "Hospitals",  new { area = "Admin" });
            if (roles.Contains(WebSiteRoles.Website_Doctor))  return RedirectToAction("Index", "Doctors",    new { area = "Doctor" });
            if (roles.Contains(WebSiteRoles.Website_Patient)) return RedirectToAction("Index", "Home",       new { area = "Patient" });
            return RedirectToAction("Index", "Home");
        }
    }

    // ── VIEW MODELS ───────────────────────────────────────────────────────────

    public class LoginVm
    {
        [Required, EmailAddress] public string Email    { get; set; } = "";
        [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
        [Display(Name = "Remember me")] public bool RememberMe { get; set; }
    }

    public class RegisterVm
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string Name  { get; set; } = "";
        [Required, DataType(DataType.Password), MinLength(6)] public string Password { get; set; } = "";
        [Required, DataType(DataType.Password), Compare("Password")] public string ConfirmPassword { get; set; } = "";
        public Gender Gender   { get; set; } = Gender.Other;
        public string? Address { get; set; }
        [DataType(DataType.Date)] public DateTime DOB { get; set; } = DateTime.Today.AddYears(-25);
    }
}
