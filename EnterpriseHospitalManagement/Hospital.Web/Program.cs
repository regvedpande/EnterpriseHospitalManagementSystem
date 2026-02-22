// Program.cs
using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Serilog;

using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services;
using Hospital.Services.Interfaces;
using Hospital.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: Serilog.RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// MVC + Razor Pages (Razor Pages is REQUIRED for Identity UI)
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        // Area paths — {2}=Area, {1}=Controller, {0}=Action
        options.AreaViewLocationFormats.Clear();
        options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/Hospital.Web/Areas/{2}/Views/{1}/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/Hospital.Web/Areas/{2}/Views/Shared/{0}.cshtml");

        // Non-area paths
        options.ViewLocationFormats.Insert(0, "/Hospital.Web/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Insert(1, "/Hospital.Web/Views/Shared/{0}.cshtml");
    });

// REQUIRED: without this, /Identity/Account/Login returns 404
builder.Services.AddRazorPages();

// DbContext
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(conn))
{
    Log.Warning("DefaultConnection missing. Add it to appsettings.json — see appsettings.json output file.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(!string.IsNullOrWhiteSpace(conn)
        ? conn
        : "Server=(localdb)\\mssqllocaldb;Database=EnterpriseHospitalManagement;Trusted_Connection=True;");
});

// Identity — AddDefaultUI() enables the built-in Login/Register Razor Pages
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedEmail = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()           // REQUIRED: scaffolds /Identity/Account/Login, /Register, etc.
    .AddDefaultTokenProviders();

// Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key");
var jwtIssuer = jwtSection.GetValue<string>("Issuer");
var jwtAudience = jwtSection.GetValue<string>("Audience");

if (string.IsNullOrWhiteSpace(jwtKey))
{
    Log.Warning("JWT Key missing. Using a temporary dev key. Set Jwt:Key in appsettings.json for production.");
    jwtKey = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
           + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
}
jwtIssuer ??= "localhost";
jwtAudience ??= "localhost";

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // set true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
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

// Cookie redirects for Identity UI
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// DB initializer — skip entirely if no connection string to avoid startup crash
using (var scope = app.Services.CreateScope())
{
    if (!string.IsNullOrWhiteSpace(conn))
    {
        try
        {
            var initializer = scope.ServiceProvider.GetService<IDbInitializer>();
            initializer?.Initialize();
            Log.Information("Database initializer executed successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database initializer failed.");
        }
    }
    else
    {
        Log.Warning("Skipping DB initialization — DefaultConnection not set in appsettings.json.");
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

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Maps /Identity/Account/Login, /Identity/Account/Register, etc.
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