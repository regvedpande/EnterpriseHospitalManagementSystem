using Hospital.Models;
using Hospital.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hospital.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser>  _um;
        private readonly SignInManager<ApplicationUser> _sm;

        public ProfileController(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm)
        { _um = um; _sm = sm; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _um.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");
            return View(new ProfileViewModel(user));
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _um.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");
            return View(new EditProfileViewModel(user));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var user = await _um.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            user.Name        = vm.Name;
            user.Address     = vm.Address;
            user.DOB         = vm.DOB;
            user.Gender      = vm.Gender;
            user.Nationality = vm.Nationality;
            user.PhoneNumber = vm.PhoneNumber;

            var result = await _um.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            TempData["success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var user = await _um.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var result = await _um.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            await _sm.RefreshSignInAsync(user);
            TempData["success"] = "Password changed successfully.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ── View Models ──────────────────────────────────────────────────────────
    public class ProfileViewModel
    {
        public string Id          { get; set; } = "";
        public string Name        { get; set; } = "";
        public string Email       { get; set; } = "";
        public string? Phone      { get; set; }
        public string Address     { get; set; } = "";
        public DateTime DOB       { get; set; }
        public Gender Gender      { get; set; }
        public string? Nationality { get; set; }
        public string? Specialist  { get; set; }
        public string? Role        { get; set; }

        public ProfileViewModel() { }
        public ProfileViewModel(ApplicationUser u)
        {
            Id = u.Id; Name = u.Name; Email = u.Email ?? "";
            Phone = u.PhoneNumber; Address = u.Address;
            DOB = u.DOB; Gender = u.Gender; Nationality = u.Nationality;
            Specialist = u.Specialist; Role = u.Role;
        }
    }

    public class EditProfileViewModel
    {
        [Required] public string Name { get; set; } = "";
        [Required] public string Address { get; set; } = "";
        [Required, DataType(DataType.Date)] public DateTime DOB { get; set; }
        [Required] public Gender Gender { get; set; }
        public string? Nationality { get; set; }
        [Phone] public string? PhoneNumber { get; set; }

        public EditProfileViewModel() { }
        public EditProfileViewModel(ApplicationUser u)
        {
            Name = u.Name; Address = u.Address;
            DOB = u.DOB; Gender = u.Gender;
            Nationality = u.Nationality; PhoneNumber = u.PhoneNumber;
        }
    }

    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password), Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = "";

        [Required, DataType(DataType.Password), Display(Name = "New Password"), MinLength(6)]
        public string NewPassword { get; set; } = "";

        [Required, DataType(DataType.Password), Display(Name = "Confirm Password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
