using Hospital.Models;
using Hospital.Repositories;
using Hospital.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

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

            // Seed all roles
            var roles = new[]
            {
                WebSiteRoles.Website_Admin,
                WebSiteRoles.Website_Doctor,
                WebSiteRoles.Website_Patient,
                WebSiteRoles.Website_Nurse,
                WebSiteRoles.Website_Pharmacist,
                WebSiteRoles.Website_LabTech,
                WebSiteRoles.Website_Receptionist,
                WebSiteRoles.Website_Accountant
            };

            foreach (var role in roles)
            {
                if (!_roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                }
            }

            // Seed default users for each role
            SeedUser("admin@hospital.com", "Admin@123", "System Admin", WebSiteRoles.Website_Admin, false);
            SeedUser("doctor@hospital.com", "Doctor@123", "Dr. Sarah Johnson", WebSiteRoles.Website_Doctor, true, "Cardiology");
            SeedUser("patient@hospital.com", "Patient@123", "John Smith", WebSiteRoles.Website_Patient, false);
            SeedUser("nurse@hospital.com", "Nurse@123", "Emily Davis", WebSiteRoles.Website_Nurse, false);
            SeedUser("pharmacist@hospital.com", "Pharmacist@123", "Michael Brown", WebSiteRoles.Website_Pharmacist, false);
            SeedUser("labtech@hospital.com", "LabTech@123", "Lisa Wilson", WebSiteRoles.Website_LabTech, false);
            SeedUser("receptionist@hospital.com", "Receptionist@123", "Anna Taylor", WebSiteRoles.Website_Receptionist, false);
            SeedUser("accountant@hospital.com", "Accountant@123", "Robert Martinez", WebSiteRoles.Website_Accountant, false);

            // Seed a default department
            if (!_db.Departments.Any())
            {
                _db.Departments.AddRange(
                    new Department { Name = "General Medicine", Description = "General medical consultations and treatments" },
                    new Department { Name = "Cardiology", Description = "Heart and cardiovascular system" },
                    new Department { Name = "Orthopedics", Description = "Bone and joint disorders" },
                    new Department { Name = "Pediatrics", Description = "Medical care for infants, children, and adolescents" },
                    new Department { Name = "Neurology", Description = "Nervous system disorders" },
                    new Department { Name = "Dermatology", Description = "Skin conditions and treatments" }
                );
                _db.SaveChanges();
            }
        }

        private void SeedUser(string email, string password, string name, string role, bool isDoctor, string? specialist = null)
        {
            if (_userManager.FindByEmailAsync(email).GetAwaiter().GetResult() == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Name = name,
                    Gender = Models.Enums.Gender.Other,
                    Address = "Hospital HQ",
                    DOB = new DateTime(1990, 1, 1),
                    IsDoctor = isDoctor,
                    Specialist = specialist,
                    EmailConfirmed = true
                };

                var result = _userManager.CreateAsync(user, password).GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(user, role).GetAwaiter().GetResult();
                }
            }
        }
    }
}