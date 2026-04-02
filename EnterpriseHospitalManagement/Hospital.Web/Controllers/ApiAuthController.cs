using System.Security.Claims;
using Hospital.Models;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [IgnoreAntiforgeryToken]
    public class ApiAuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly UserManager<ApplicationUser> _users;
        private readonly JwtService _jwt;

        public ApiAuthController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, JwtService jwt)
        {
            _signIn = signIn;
            _users = users;
            _jwt = jwt;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _users.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials." });

            var result = await _signIn.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid credentials." });

            var roles = await _users.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            return Ok(new
            {
                token = _jwt.GenerateToken(user, role),
                expiresInMinutes = 60,
                user = BuildUserPayload(user, role)
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var user = await _users.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            var roles = await _users.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            return Ok(BuildUserPayload(user, role));
        }

        private static object BuildUserPayload(ApplicationUser user, string role)
        {
            var displayName = string.IsNullOrWhiteSpace(user.Name) ? (user.Email ?? user.UserName ?? "User") : user.Name;
            return new
            {
                id = user.Id,
                name = displayName,
                email = user.Email ?? user.UserName ?? "",
                role,
                roleDisplay = GetRoleDisplay(role),
                defaultRoute = "/dashboard",
                initials = BuildInitials(displayName)
            };
        }

        private static string GetRoleDisplay(string role)
        {
            return role switch
            {
                WebSiteRoles.Website_Admin => "Administrator",
                WebSiteRoles.Website_Doctor => "Doctor",
                WebSiteRoles.Website_Patient => "Patient",
                WebSiteRoles.Website_Nurse => "Nurse",
                WebSiteRoles.Website_Pharmacist => "Pharmacist",
                WebSiteRoles.Website_LabTech => "Lab Technician",
                WebSiteRoles.Website_Receptionist => "Receptionist",
                WebSiteRoles.Website_Accountant => "Accountant",
                _ => "User"
            };
        }

        private static string BuildInitials(string displayName)
        {
            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return "U";

            if (parts.Length == 1)
                return parts[0][0].ToString().ToUpperInvariant();

            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";
        }
    }
}
