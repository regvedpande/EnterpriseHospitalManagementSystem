using System.ComponentModel.DataAnnotations;
using Hospital.Models;
using Hospital.Models.Enums;
using Hospital.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly UserManager<ApplicationUser>  _users;
        private readonly IEmailSender _email;
        private readonly ISmsService  _sms;

        public AuthController(
            SignInManager<ApplicationUser> signIn,
            UserManager<ApplicationUser>  users,
            IEmailSender email,
            ISmsService  sms)
        {
            _signIn = signIn;
            _users  = users;
            _email  = email;
            _sms    = sms;
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
                PhoneNumber    = model.PhoneNumber,
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

            // Send welcome email (silently skipped if SMTP not configured)
            await _email.SendEmailAsync(user.Email!, "Welcome to MedCore HMS",
                $"<h2>Welcome, {user.Name}!</h2>" +
                $"<p>Your patient account has been successfully created at <strong>MedCore HMS</strong>.</p>" +
                $"<p>You can now book appointments, view your lab results, bills, and medical reports through your patient portal.</p>" +
                $"<p>If you have any questions, please contact us at <a href='mailto:support@medcorehms.com'>support@medcorehms.com</a>.</p>" +
                $"<br/><p>The MedCore HMS Team</p>");

            // Send welcome SMS (silently skipped if Twilio not configured or no phone)
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                await _sms.SendSmsAsync(user.PhoneNumber,
                    $"Welcome to MedCore HMS, {user.Name}! Your patient account is ready. Log in to book appointments and view your health records.");
            }

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
            if (User.IsInRole(WebSiteRoles.Website_Admin))        return RedirectToAction("Index", "Home",         new { area = "Admin" });
            if (User.IsInRole(WebSiteRoles.Website_Doctor))       return RedirectToAction("Index", "Home",         new { area = "Doctor" });
            if (User.IsInRole(WebSiteRoles.Website_Patient))      return RedirectToAction("Index", "Home",         new { area = "Patient" });
            if (User.IsInRole(WebSiteRoles.Website_Nurse))        return RedirectToAction("Index", "Home",         new { area = "Nurse" });
            if (User.IsInRole(WebSiteRoles.Website_Pharmacist))   return RedirectToAction("Index", "Home",         new { area = "Pharmacist" });
            if (User.IsInRole(WebSiteRoles.Website_LabTech))      return RedirectToAction("Index", "Home",         new { area = "LabTech" });
            if (User.IsInRole(WebSiteRoles.Website_Receptionist)) return RedirectToAction("Index", "Home",         new { area = "Receptionist" });
            if (User.IsInRole(WebSiteRoles.Website_Accountant))   return RedirectToAction("Index", "Home",         new { area = "Accountant" });
            return RedirectToAction("Index", "Home");
        }

        private async Task<IActionResult> RoleRedirectAsync(ApplicationUser user)
        {
            var roles = await _users.GetRolesAsync(user);
            if (roles.Contains(WebSiteRoles.Website_Admin))        return RedirectToAction("Index", "Home",         new { area = "Admin" });
            if (roles.Contains(WebSiteRoles.Website_Doctor))       return RedirectToAction("Index", "Home",         new { area = "Doctor" });
            if (roles.Contains(WebSiteRoles.Website_Patient))      return RedirectToAction("Index", "Home",         new { area = "Patient" });
            if (roles.Contains(WebSiteRoles.Website_Nurse))        return RedirectToAction("Index", "Home",         new { area = "Nurse" });
            if (roles.Contains(WebSiteRoles.Website_Pharmacist))   return RedirectToAction("Index", "Home",         new { area = "Pharmacist" });
            if (roles.Contains(WebSiteRoles.Website_LabTech))      return RedirectToAction("Index", "Home",         new { area = "LabTech" });
            if (roles.Contains(WebSiteRoles.Website_Receptionist)) return RedirectToAction("Index", "Home",         new { area = "Receptionist" });
            if (roles.Contains(WebSiteRoles.Website_Accountant))   return RedirectToAction("Index", "Home",         new { area = "Accountant" });
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
        [Phone, Display(Name = "Phone Number")] public string? PhoneNumber { get; set; }
    }
}
