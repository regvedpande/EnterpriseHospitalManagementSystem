using Hospital.Models;
using Hospital.Models.Enums;
using Hospital.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Hospital.Utilities
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public void Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // log migration failure
                throw;
            }

            if (_roleManager.RoleExistsAsync(WebSiteRoles.Website_Admin).GetAwaiter().GetResult())
            {
                return;
            }

            _roleManager.CreateAsync(new IdentityRole(WebSiteRoles.Website_Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(WebSiteRoles.Website_Patient)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(WebSiteRoles.Website_Doctor)).GetAwaiter().GetResult();

            var admin = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                Name = "System Administrator",
                Address = "Head Office",
                DOB = DateTime.Now.AddYears(-30),
                Gender = Gender.Male,
                IsDoctor = false,
                EmailConfirmed = true
            };

            _userManager.CreateAsync(admin, "Admin@123").GetAwaiter().GetResult();

            var adminUser = _context.Users.FirstOrDefault(u => u.Email == "admin@example.com");
            if (adminUser != null)
            {
                _userManager.AddToRoleAsync(adminUser, WebSiteRoles.Website_Admin).GetAwaiter().GetResult();
            }
        }
    }
}
