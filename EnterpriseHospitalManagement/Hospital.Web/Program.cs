using System;
using System.Text;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services;
using Hospital.Services.Interfaces;
using Hospital.Utilities;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog configuration ---
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: Serilog.RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add MVC
builder.Services.AddControllersWithViews();

// --- Database: use SQL Server if a connection string exists, otherwise fall back to InMemory
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(defaultConn))
{
    Log.Warning("No DefaultConnection found. Using InMemory DB for development.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("Dev_EnterpriseHospitalDB"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(defaultConn));
}

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedEmail = false; // set true in production
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --- Register repository / unit of work implementations ---
// Open-generic registration for repository (ensure your GenericRepository<> lives in Hospital.Repositories)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Unit of work (ensure interface IUnitOfWork and UnitOfWork class exist)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- JWT config (defensive: generate a temporary key for dev if missing) ---
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key");
var jwtIssuer = jwtSection.GetValue<string>("Issuer");
var jwtAudience = jwtSection.GetValue<string>("Audience");

// If no key found, generate a throwaway development key so app doesn't crash.
// NOTE: for production, set Jwt:Key in appsettings or environment variables.
if (string.IsNullOrWhiteSpace(jwtKey))
{
    Log.Warning("JWT Key was missing from configuration. A temporary development key will be used. Set Jwt:Key in appsettings.json or environment variables for a stable key in production.");
    // Generate a reasonably strong temporary key (base64).
    jwtKey = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
}

if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    Log.Warning("Jwt:Issuer or Jwt:Audience missing from config. Defaulting to 'localhost' values for development only.");
    jwtIssuer ??= "localhost";
    jwtAudience ??= "localhost";
}

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// Authentication / JwtBearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
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
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero
    };
});

// application DI - services
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IHospitalInfoService, HospitalInfoService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

// utilities / other services
builder.Services.AddScoped<ImageOperations>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<SftpService>();

// Email sender implementation for Identity UI's IEmailSender
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();

// add IHttpContextAccessor if needed
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// ensure DB (run initializer) - NOTE: make sure you have a single IDbInitializer interface and DbInitializer implementation
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    initializer.Initialize();
}

// pipeline
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

// Areas route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

