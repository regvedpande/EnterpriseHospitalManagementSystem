// Program.cs
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog configuration ---
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: Serilog.RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("Starting web host...");

// Add services to container
builder.Services.AddControllersWithViews();

// EF + Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedEmail = false; // set true in production
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Jwt config (robust handling if values missing)
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key");
var jwtIssuer = jwtSection.GetValue<string>("Issuer");
var jwtAudience = jwtSection.GetValue<string>("Audience");

// If Key missing, generate a development fallback but log a strong warning.
// In production you must supply a stable strong key via configuration.
if (string.IsNullOrWhiteSpace(jwtKey))
{
    var fallbackKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"); // ~64 char
    jwtKey = fallbackKey.Substring(0, 32); // at least 256-bit when encoded (32 bytes)
    Log.Warning("JWT Key was missing from configuration. A temporary development key has been generated. " +
                "Set Jwt:Key in appsettings.json or environment variables for a stable key in production.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    Log.Warning("Jwt:Issuer or Jwt:Audience missing from config. Defaulting to 'localhost' values for development only.");
    jwtIssuer ??= "localhost";
    jwtAudience ??= "localhost";
}

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
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

// application DI - repositories, services, utilities
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IHospitalInfoService, HospitalInfoService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<ImageOperations>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<SftpService>();

// Correct interface for the EmailSender implementation:
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();

// add IHttpContextAccessor if needed
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// ensure DB - catch and log exceptions clearly so they don't silently crash
using (var scope = app.Services.CreateScope())
{
    try
    {
        var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        initializer.Initialize();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Database initializer failed during app startup.");
        // rethrow so the stack trace appears in console; comment this out if you prefer host to keep alive
        throw;
    }
}

// pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Areas route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Log.Information("Starting app...");
app.Run();
