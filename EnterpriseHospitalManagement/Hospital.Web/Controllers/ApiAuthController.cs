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
    public class ApiAuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly UserManager<ApplicationUser>  _users;
        private readonly JwtService _jwt;

        public ApiAuthController(SignInManager<ApplicationUser> s, UserManager<ApplicationUser> u, JwtService j)
        { _signIn = s; _users = u; _jwt = j; }

        public class LoginRequest { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }

        [HttpPost("login"), AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest m)
        {
            var user = await _users.FindByEmailAsync(m.Email);
            if (user == null) return Unauthorized("Invalid credentials");
            var result = await _signIn.CheckPasswordSignInAsync(user, m.Password, false);
            if (!result.Succeeded) return Unauthorized("Invalid credentials");
            var roles = await _users.GetRolesAsync(user);
            return Ok(new { token = _jwt.GenerateToken(user, roles.FirstOrDefault() ?? ""), expiresInMinutes = 60 });
        }
    }
}
