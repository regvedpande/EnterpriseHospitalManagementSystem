using Hospital.Models;
using Hospital.Models.Enums;
using Hospital.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hospital.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ── LOGIN ─────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDashboard();

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToDashboard(user);
        }

        // ── REGISTER ──────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDashboard();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                Gender = model.Gender,
                Address = model.Address ?? "",
                DOB = model.DOB,
                IsDoctor = false,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // Default role for self-registered users is Patient
            await _userManager.AddToRoleAsync(user, WebSiteRoles.Website_Patient);
            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["success"] = "Registration successful. Welcome!";
            return RedirectToAction("Index", "Home", new { area = "Patient" });
        }

        // ── LOGOUT ────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ── HELPERS ───────────────────────────────────────────────────────────

        /// <summary>Re-query roles from DB for a freshly-logged-in user.</summary>
        private IActionResult RedirectToDashboard(ApplicationUser? user = null)
        {
            // After login the claims are set; use User.IsInRole for redirects
            if (User.IsInRole(WebSiteRoles.Website_Admin))
                return RedirectToAction("Index", "Hospitals", new { area = "Admin" });

            if (User.IsInRole(WebSiteRoles.Website_Doctor))
                return RedirectToAction("Index", "Doctors", new { area = "Doctor" });

            if (User.IsInRole(WebSiteRoles.Website_Patient))
                return RedirectToAction("Index", "Home", new { area = "Patient" });

            // Fallback — no role assigned yet
            return RedirectToAction("Index", "Home");
        }
    }

    // ── VIEW MODELS ───────────────────────────────────────────────────────────

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Name { get; set; } = "";

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = "";

        public Gender Gender { get; set; } = Gender.Other;

        public string? Address { get; set; }

        [DataType(DataType.Date)]
        public DateTime DOB { get; set; } = DateTime.Today.AddYears(-25);
    }
}