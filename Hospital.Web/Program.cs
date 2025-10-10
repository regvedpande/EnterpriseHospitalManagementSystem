using Hospital.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Hospital.Utilities;
using Hospital.Repositories.Implementation;
using Hospital.Models;
using Microsoft.AspNetCore.Identity.UI.Services; // For ApplicationUser
using Hospital.Services;

namespace Hospital.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Controllers with Views (for MVC)
            builder.Services.AddControllersWithViews();

            // Configure DbContext with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure ASP.NET Identity with custom ApplicationUser
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Register custom services (scoped/transient as appropriate)
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient<IHospitalInfo, HospitalInfoService>();
            builder.Services.AddTransient<IRoomService, RoomService>();

            // Add support for Razor Pages (for Identity UI)
            builder.Services.AddRazorPages();

            // Build the app
            var app = builder.Build();

            // Seed the database with roles and admin user
            DataSeeding(app);

            // Configure middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Authentication must come before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Razor Pages and default MVC route (starts in Patient area)
            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{Area=admin}/{controller=Hospitals}/{action=Index}/{id?}");

            // Run the app
            app.Run();
        }

        // Static method to seed data
        private static void DataSeeding(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            dbInitializer.Initialize();
        }
    }
}