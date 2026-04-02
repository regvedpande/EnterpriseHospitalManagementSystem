using System.Text;
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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 50_000_000)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllersWithViews(opts =>
    {
        opts.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    })
    .AddRazorOptions(options =>
    {
        options.AreaViewLocationFormats.Insert(0, "/Hospital.Web/Areas/{2}/Views/{1}/{0}.cshtml");
        options.AreaViewLocationFormats.Insert(1, "/Hospital.Web/Areas/{2}/Views/Shared/{0}.cshtml");
        options.ViewLocationFormats.Insert(0, "/Hospital.Web/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Insert(1, "/Hospital.Web/Views/Shared/{0}.cshtml");
    });
builder.Services.AddRazorPages();

builder.Services.AddResponseCompression(opts => opts.EnableForHttps = true);
builder.Services.AddResponseCaching();

var allowedOrigins = ResolveAllowedOrigins(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => IsAllowedOrigin(origin, allowedOrigins))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var dbProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var sqlServerConn = builder.Configuration.GetConnectionString("DefaultConnection");
var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection");
if (string.IsNullOrWhiteSpace(sqliteConn))
{
    var sqlitePath = builder.Configuration["Database:SqlitePath"]
        ?? Path.Combine(builder.Environment.ContentRootPath, "data", "medcore-hms.db");
    Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);
    sqliteConn = $"Data Source={sqlitePath}";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(sqliteConn);
        return;
    }

    options.UseSqlServer(
        !string.IsNullOrWhiteSpace(sqlServerConn)
            ? sqlServerConn
            : "Server=(localdb)\\mssqllocaldb;Database=EnterpriseHospitalManagement;Trusted_Connection=True;",
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(30);
        });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

var redisConn = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConn))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
        options.InstanceName = "MedCoreHMS:";
    });
    Log.Information("Redis cache configured at {Host}", redisConn.Split(',')[0]);
}
else
{
    builder.Services.AddDistributedMemoryCache();
    Log.Information("Redis not configured - using in-memory distributed cache");
}
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

builder.Services.AddHttpClient("nvidia", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddSingleton<IAiService, NvidiaAiService>();

builder.Services.AddSingleton<IMessageBus, RabbitMqMessageBus>();
builder.Services.AddHostedService<RabbitMqConsumerService>();

builder.Services.AddScoped<IGenericRepository<Hospital.Models.ApplicationUser>, GenericRepository<Hospital.Models.ApplicationUser>>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "localhost";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "localhost";

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

builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IHospitalInfoService, HospitalInfoService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddScoped<ILabService, LabService>();
builder.Services.AddScoped<IPatientReportService, PatientReportService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IAiAssistantService, AiAssistantService>();
builder.Services.AddScoped<ImageOperations>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<SftpService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
builder.Services.AddScoped<Hospital.Utilities.ISmsService, Hospital.Utilities.TwilioSmsService>();
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    options.Cookie.HttpOnly = true;
});

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        scope.ServiceProvider.GetService<IDbInitializer>()?.Initialize();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "DB init failed");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseForwardedHeaders();
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    ctx.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");
app.UseResponseCompression();
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000");
    }
});
app.UseRouting();
app.UseCors("Frontend");
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

static string[] ResolveAllowedOrigins(IConfiguration configuration)
{
    var configured = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    if (configured is { Length: > 0 })
        return configured;

    var raw = configuration["Cors:AllowedOrigins"];
    if (!string.IsNullOrWhiteSpace(raw))
    {
        var parsed = raw
            .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (parsed.Length > 0)
            return parsed;
    }

    return new[]
    {
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "https://*.vercel.app"
    };
}

static bool IsAllowedOrigin(string origin, IReadOnlyCollection<string> configuredOrigins)
{
    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        return false;

    foreach (var configured in configuredOrigins)
    {
        if (string.IsNullOrWhiteSpace(configured))
            continue;

        if (configured.Contains('*'))
        {
            if (!Uri.TryCreate(configured.Replace("*.", "placeholder."), UriKind.Absolute, out var wildcardUri))
                continue;

            if (!string.Equals(uri.Scheme, wildcardUri.Scheme, StringComparison.OrdinalIgnoreCase))
                continue;

            var wildcardHost = wildcardUri.Host.Replace("placeholder.", "", StringComparison.OrdinalIgnoreCase);
            if (uri.Host.EndsWith($".{wildcardHost}", StringComparison.OrdinalIgnoreCase))
                return true;

            continue;
        }

        if (string.Equals(origin, configured, StringComparison.OrdinalIgnoreCase))
            return true;
    }

    return false;
}
