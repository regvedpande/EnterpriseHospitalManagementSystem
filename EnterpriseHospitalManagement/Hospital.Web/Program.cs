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

// Add controllers + views
builder.Services.AddControllersWithViews();

var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(defaultConn))
{
    Log.Warning("No DefaultConnection found. Using InMemory DB for development.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("Dev_EnterpriseHospitalDB");
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

// --- Register repository open-generic and UnitOfWork implementations ---
// Open-generic registration for IGenericRepository<T>
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Unit of Work registration (fully-qualified if required by your project)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- JWT config (defensive: don't crash if config missing) ---
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key");
var jwtIssuer = jwtSection.GetValue<string>("Issuer");
var jwtAudience = jwtSection.GetValue<string>("Audience");

// If no key found, generate a throwaway development key so app doesn't crash.
// Note: this is only for development convenience — set a stable key in config for real usage.
if (string.IsNullOrWhiteSpace(jwtKey))
{
    Log.Warning("JWT Key was missing from configuration. A temporary development key will be used. Set Jwt:Key in appsettings.json or environment variables for a stable key in production.");
    jwtKey = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
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

// --- application DI - services & utilities ---
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

// --- Run DB initializer (safe: guarded so app won't crash if DB unavailable) ---
try
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;

    // Resolve initializer if available (it should be registered above)
    var initializer = sp.GetService<IDbInitializer>();
    if (initializer is not null)
    {
        try
        {
            initializer.Initialize();
        }
        catch (Exception initEx)
        {
            Log.Error(initEx, "DbInitializer threw an exception during Initialize(). Continuing without DB initialization.");
        }
    }
    else
    {
        Log.Warning("IDbInitializer service not registered. Skipping DB initialization.");
    }
}
catch (Exception ex)
{
    // Catch any DI/build-time issues gracefully so the app doesn't crash on startup
    Log.Error(ex, "Exception while attempting to run DB initializer scope. Continuing startup.");
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
