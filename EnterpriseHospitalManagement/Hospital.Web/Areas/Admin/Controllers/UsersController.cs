using Hospital.Models;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser>  _users;
        private readonly RoleManager<IdentityRole>     _roles;

        public UsersController(UserManager<ApplicationUser> u, RoleManager<IdentityRole> r) { _users = u; _roles = r; }

        public async Task<IActionResult> Index(int page = 1, int size = 10)
        {
            var all   = await _users.Users.OrderBy(u => u.Name).ToListAsync();
            var total = all.Count;
            var items = all.Skip((page - 1) * size).Take(size).ToList();
            var vms   = new List<UserRoleVm>();
            foreach (var u in items)
                vms.Add(new UserRoleVm { User = u, Roles = (await _users.GetRolesAsync(u)).ToList() });

            ViewBag.Page      = page;
            ViewBag.Size      = size;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)size);
            return View(vms);
        }

        [HttpGet]
        public async Task<IActionResult> EditRoles(string id)
        {
            var user  = await _users.FindByIdAsync(id);
            if (user == null) return NotFound();
            var allRoles  = await _roles.Roles.Select(r => r.Name!).ToListAsync();
            var userRoles = await _users.GetRolesAsync(user);
            ViewBag.AllRoles  = allRoles;
            ViewBag.UserRoles = userRoles;
            return View(user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(string id, List<string> selectedRoles)
        {
            var user      = await _users.FindByIdAsync(id);
            if (user == null) return NotFound();
            var current   = await _users.GetRolesAsync(user);
            await _users.RemoveFromRolesAsync(user, current);
            if (selectedRoles?.Count > 0)
                await _users.AddToRolesAsync(user, selectedRoles);
            TempData["success"] = "Roles updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _users.FindByIdAsync(id);
            if (user != null) await _users.DeleteAsync(user);
            TempData["success"] = "User deleted.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class UserRoleVm
    {
        public ApplicationUser User  { get; set; } = null!;
        public List<string>    Roles { get; set; } = new();
    }
}
