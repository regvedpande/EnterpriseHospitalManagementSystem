using EnterpriseHospitalManagement.Hospital.Repositories;
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

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseSqlServer(!string.IsNullOrWhiteSpace(conn) ? conn
        : "Server=(localdb)\\mssqllocaldb;Database=EnterpriseHospitalManagement;Trusted_Connection=True;"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
{
    o.Password.RequireDigit = true;
    o.Password.RequiredLength = 6;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequireUppercase = false;
    o.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Repositories ─────────────────────────────────────────────────────────────
// Use the concrete UnitOfWork directly — avoids the generic-constraint error
// CS0311 that occurs when IUnitOfWork and UnitOfWork live in different assemblies.
builder.Services.AddScoped<IGenericRepository<Hospital.Models.ApplicationUser>,
                            GenericRepository<Hospital.Models.ApplicationUser>>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── JWT ───────────────────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? Convert.ToBase64String(Guid.NewGuid().ToByteArray())
     + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "localhost";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "localhost";

// Cookie = default for MVC; JWT = explicit for /api/* only
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    o.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    o.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
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

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IHospitalInfoService, HospitalInfoService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ImageOperations>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<SftpService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
builder.Services.AddHttpContextAccessor();

// ── Cookie ────────────────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Auth/Login";
    o.LogoutPath = "/Auth/Logout";
    o.AccessDeniedPath = "/Auth/AccessDenied";
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
    o.SlidingExpiration = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try { scope.ServiceProvider.GetService<IDbInitializer>()?.Initialize(); }
    catch (Exception ex) { Log.Error(ex, "DB init failed"); }
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

app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();