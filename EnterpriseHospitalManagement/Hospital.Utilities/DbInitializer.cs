using Hospital.Models;
using Hospital.Repositories;
using Hospital.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hospital.Utilities
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            // Apply pending migrations automatically
            try
            {
                _db.Database.Migrate();
            }
            catch (Exception)
            {
                // Migration might already be up-to-date
            }

            // Seed roles
            if (!_roleManager.RoleExistsAsync(WebSiteRoles.Website_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(WebSiteRoles.Website_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(WebSiteRoles.Website_Doctor)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(WebSiteRoles.Website_Patient)).GetAwaiter().GetResult();
            }

            // Seed default admin user
            var adminEmail = "admin@hospital.com";
            if (_userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult() == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "System Admin",
                    Gender = Models.Enums.Gender.Other,
                    Address = "Hospital HQ",
                    DOB = new DateTime(1990, 1, 1),
                    IsDoctor = false,
                    EmailConfirmed = true
                };

                var result = _userManager.CreateAsync(admin, "Admin@123").GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(admin, WebSiteRoles.Website_Admin).GetAwaiter().GetResult();
                }
            }
        }
    }
}