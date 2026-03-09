// Program.cs
using EnterpriseHospitalManagement.Hospital.Repositories;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Text;

namespace EnterpriseHospitalManagement
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Serilog
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/log-.txt", rollingInterval: Serilog.RollingInterval.Day)
                .CreateLogger();
            builder.Host.UseSerilog();

            // MVC + Razor Pages
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // DbContext
            var conn = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(conn))
                Log.Warning("DefaultConnection missing. Add it to appsettings.json.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(!string.IsNullOrWhiteSpace(conn)
                    ? conn
                    : "Server=(localdb)\\mssqllocaldb;Database=EnterpriseHospitalManagement;Trusted_Connection=True;");
            });

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            // Repositories
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // JWT — registered as additional scheme only, used explicitly by API controllers
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "localhost";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "localhost";

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                Log.Warning("JWT Key missing. Using temporary dev key.");
                jwtKey = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                       + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            }

            // ✅ KEY FIX: Identity cookie is default scheme for MVC pages.
            // JWT is an additional scheme used only by [Authorize(AuthenticationSchemes = "Bearer")] on API controllers.
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Application services
            builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
            builder.Services.AddScoped<IContactService, ContactService>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();
            builder.Services.AddScoped<IRoomService, RoomService>();
            builder.Services.AddScoped<IHospitalInfoService, HospitalInfoService>();
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();

            // Utilities
            builder.Services.AddScoped<ImageOperations>();
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<SftpService>();

            builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
            builder.Services.AddHttpContextAccessor();

            // ✅ Cookie redirects to your AuthController, not Identity UI
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/Login";
            });

            var app = builder.Build();

            // DB initializer
            using (var scope = app.Services.CreateScope())
            {
                if (!string.IsNullOrWhiteSpace(conn))
                {
                    try
                    {
                        scope.ServiceProvider.GetService<IDbInitializer>()?.Initialize();
                        Log.Information("Database initialized successfully.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Database initialization failed.");
                    }
                }
                else
                {
                    Log.Warning("Skipping DB initialization — DefaultConnection not set.");
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Area route MUST come before default
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}