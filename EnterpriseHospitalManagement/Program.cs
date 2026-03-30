using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.Web.Infrastructure.AI;
using Hospital.Web.Infrastructure.Caching;
using Hospital.Web.Infrastructure.Claims;
using Hospital.Web.Infrastructure.Messaging;
using Hospital.Web.Infrastructure.Queue;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ───────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30, fileSizeLimitBytes: 50_000_000)
    .CreateLogger();
builder.Host.UseSerilog();

// ── MVC + Razor ───────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(opts =>
    {
        // Global anti-forgery filter — all POST/PUT/DELETE require valid token
        opts.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    })
    .AddRazorOptions(options =>
    {
        // Insert Hospital.Web paths FIRST so they take precedence over root-level duplicates
        options.AreaViewLocationFormats.Insert(0, "/Hospital.Web/Areas/{2}/Views/{1}/{0}.cshtml");
        options.AreaViewLocationFormats.Insert(1, "/Hospital.Web/Areas/{2}/Views/Shared/{0}.cshtml");
        options.ViewLocationFormats.Insert(0, "/Hospital.Web/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Insert(1, "/Hospital.Web/Views/Shared/{0}.cshtml");
    });
builder.Services.AddRazorPages();

// ── Response Compression + Caching headers ────────────────────────────────────
builder.Services.AddResponseCompression(opts => opts.EnableForHttps = true);
builder.Services.AddResponseCaching();

// ── Database ──────────────────────────────────────────────────────────────────
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(o =>
{
    o.UseSqlServer(
        !string.IsNullOrWhiteSpace(conn) ? conn
            : "Server=(localdb)\\mssqllocaldb;Database=EnterpriseHospitalManagement;Trusted_Connection=True;",
        sqlOpts =>
        {
            sqlOpts.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);  // Polly-style retry built into EF Core SQL provider
            sqlOpts.CommandTimeout(30);
        });
});

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
{
    o.Password.RequireDigit           = true;
    o.Password.RequiredLength         = 6;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequireUppercase       = false;
    o.SignIn.RequireConfirmedEmail     = false;
    // Account lock-out after 5 failed attempts for 5 minutes
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(5);
    o.Lockout.AllowedForNewUsers      = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>(); // adds "FullName" claim

// ── Redis Distributed Cache ───────────────────────────────────────────────────
var redisConn = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConn))
{
    builder.Services.AddStackExchangeRedisCache(opts =>
    {
        opts.Configuration = redisConn;
        opts.InstanceName  = "MedCoreHMS:";
    });
    Log.Information("Redis cache configured at {Host}", redisConn.Split(',')[0]);
}
else
{
    // Fall back to in-memory distributed cache (single-node dev/staging)
    builder.Services.AddDistributedMemoryCache();
    Log.Information("Redis not configured — using in-memory distributed cache");
}
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// ── In-process Background Task Queue ─────────────────────────────────────────
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

// ── NVIDIA AI Service ─────────────────────────────────────────────────────
builder.Services.AddHttpClient("nvidia", c =>
{
    c.Timeout = TimeSpan.FromSeconds(60);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddSingleton<IAiService, NvidiaAiService>();

// ── RabbitMQ Message Bus ──────────────────────────────────────────────────────
builder.Services.AddSingleton<IMessageBus, RabbitMqMessageBus>();
builder.Services.AddHostedService<RabbitMqConsumerService>();

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IGenericRepository<Hospital.Models.ApplicationUser>,
                            GenericRepository<Hospital.Models.ApplicationUser>>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── JWT (used by /api/* endpoints; MVC uses cookies) ─────────────────────────
var jwtKey      = builder.Configuration["Jwt:Key"]
    ?? Convert.ToBase64String(Guid.NewGuid().ToByteArray())
     + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? "localhost";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "localhost";

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    o.DefaultChallengeScheme    = IdentityConstants.ApplicationScheme;
    o.DefaultSignInScheme       = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken            = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateIssuerSigningKey  = true,
        ValidIssuer              = jwtIssuer,
        ValidAudience            = jwtAudience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew                = TimeSpan.Zero
    };
});

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IContactService,         ContactService>();
builder.Services.AddScoped<IDoctorService,          DoctorService>();
builder.Services.AddScoped<IRoomService,            RoomService>();
builder.Services.AddScoped<IHospitalInfoService,    HospitalInfoService>();
builder.Services.AddScoped<IAppointmentService,     AppointmentService>();
builder.Services.AddScoped<IBillService,            BillService>();
builder.Services.AddScoped<IInsuranceService,       InsuranceService>();
builder.Services.AddScoped<ILabService,             LabService>();
builder.Services.AddScoped<IPatientReportService,   PatientReportService>();
builder.Services.AddScoped<IPayrollService,         PayrollService>();
builder.Services.AddScoped<IMedicineService,        MedicineService>();
builder.Services.AddScoped<IDepartmentService,      DepartmentService>();
builder.Services.AddScoped<ISupplierService,        SupplierService>();
builder.Services.AddScoped<IDbInitializer,          DbInitializer>();
builder.Services.AddScoped<IReportService,          ReportService>();
builder.Services.AddScoped<IDocumentService,        DocumentService>();
builder.Services.AddScoped<ImageOperations>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<SftpService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
builder.Services.AddScoped<Hospital.Utilities.ISmsService, Hospital.Utilities.TwilioSmsService>();
builder.Services.AddHttpContextAccessor();

// ── Cookie Authentication ─────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath         = "/Auth/Login";
    o.LogoutPath        = "/Auth/Logout";
    o.AccessDeniedPath  = "/Auth/AccessDenied";
    o.ExpireTimeSpan    = TimeSpan.FromHours(8);
    o.SlidingExpiration = true;
    // Security: SameSite=Strict prevents CSRF via cross-origin requests
    o.Cookie.SameSite   = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    o.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    o.Cookie.HttpOnly   = true;
});

// ── Security Headers ──────────────────────────────────────────────────────────
builder.Services.AddHsts(opts =>
{
    opts.Preload           = true;
    opts.IncludeSubDomains = true;
    opts.MaxAge            = TimeSpan.FromDays(365);
});

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── DB Init (migrations + seeding) ───────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    try { scope.ServiceProvider.GetService<IDbInitializer>()?.Initialize(); }
    catch (Exception ex) { Log.Error(ex, "DB init failed"); }
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Security headers middleware
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("X-Frame-Options",        "SAMEORIGIN");
    ctx.Response.Headers.Append("X-XSS-Protection",       "1; mode=block");
    ctx.Response.Headers.Append("Referrer-Policy",        "strict-origin-when-cross-origin");
    await next();
});

app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");
app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static assets 30 days; versioned via asp-append-version
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000");
    }
});
app.UseRouting();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("areas",   "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
