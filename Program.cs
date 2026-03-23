using EShopMVC.Hubs;
using EShopMVC.Infrastructure.Concurrency;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Infrastructure.Data.Seed;
using EShopMVC.Infrastructure.Hangfire;
using EShopMVC.Infrastructure.Jobs;
using EShopMVC.Infrastructure.Middleware;
using EShopMVC.Models;
using EShopMVC.Models.Options;
using EShopMVC.Modules.Analytics.Services;
using EShopMVC.Modules.Fraud;
using EShopMVC.Modules.Fraud.Repositories;
using EShopMVC.Modules.Fraud.Rules;
using EShopMVC.Modules.Fraud.Services;
using EShopMVC.Modules.Orders;
using EShopMVC.Modules.Orders.Application;
using EShopMVC.Modules.Orders.Application.Services;
using EShopMVC.Modules.Orders.Domain.Services;
using EShopMVC.Modules.Orders.Queries;
using EShopMVC.Modules.Payments;
using EShopMVC.Modules.Payments.Public;
using EShopMVC.Modules.Payments.Services;
using EShopMVC.Repositories.Refunds;
using EShopMVC.Services;
using EShopMVC.Services.Orders;
using EShopMVC.Shared.EventBus;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [CorrelationId:{CorrelationId}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Render port
builder.WebHost.UseUrls("http://0.0.0.0:10000");

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=eshop.db");
});

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

GlobalJobFilters.Filters.Add(
    new AutomaticRetryAttribute { Attempts = 0 });

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);
builder.Services.AddHttpClient<IyzicoRestService>();

builder.Services.Configure<EShopMVC.Models.IyzicoOptions>(
    builder.Configuration.GetSection("Iyzico")
);

builder.Services.AddSingleton<JobFailureTimelineFilter>();

builder.Services.AddSignalR();
builder.Services.AddPaymentsModule();
builder.Services.AddFraudModule(); builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

builder.Services.AddOrdersModule();

builder.Host.UseSerilog();

builder.Services.AddMemoryCache();

builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = "Çok fazla deneme yaptınız. Lütfen biraz sonra tekrar deneyin."
        });
    };

    // LOGIN
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // FORGOT PASSWORD
    options.AddPolicy("forgot", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Çok fazla deneme yaptınız. Lütfen biraz sonra tekrar deneyin."
            });
        };
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = false; // 🔥
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";          // login sayfası
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.AllowedForNewUsers = true;
});

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<IPaymentRefundGateway, IyzicoRefundService>(); // 🔥 DOĞRU

builder.Services.AddScoped<IFraudRuleService, FraudRuleService>();
builder.Services.AddScoped<IFraudRule, HighAmountRule>();
builder.Services.AddScoped<IFraudRule, TooManyOrdersRule>();
builder.Services.AddScoped<FraudDetectionService>();
builder.Services.AddScoped<IOrderDetailsService, OrderDetailsService>();
builder.Services.AddScoped<OrderTimelineService>();
builder.Services.AddScoped<OrderTimelineBuilder>();
builder.Services.AddScoped<RefundService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<OrderMailJob>();
builder.Services.AddScoped<RefundPolicy>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<OrderRiskService>();
builder.Services.AddScoped<FraudScoreService>();
builder.Services.AddScoped<FraudTimelineService>();
builder.Services.AddScoped<FraudAutoBlockService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<FraudAlertService>();
builder.Services.AddScoped<UserFraudService>();
builder.Services.AddScoped<FraudPatternService>();
builder.Services.AddScoped<FraudGraphService>();
builder.Services.AddScoped<FraudPredictionService>();
builder.Services.AddScoped<IPaymentGateway, IyzicoService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<OrdersQueryService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<FraudEvaluationService>();
builder.Services.AddScoped<FraudService>();
builder.Services.AddScoped<FraudRuleEngine>();
builder.Services.AddHostedService<OutboxProcessor>();
builder.Services.AddScoped<FraudCaseService>();
builder.Services.AddSingleton<CheckoutLockService>();
builder.Services.AddScoped<BehaviorScoreService>();
builder.Services.AddScoped<FraudRiskPipeline>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services
    .AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        // normal views
        options.ViewLocationFormats.Add("/Web/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Web/Views/Shared/{0}.cshtml");

        // ADMIN AREA views
        options.AreaViewLocationFormats.Add("/Web/Areas/{2}/Views/{1}/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/Web/Areas/{2}/Views/Shared/{0}.cshtml");
    });

builder.Services.Configure<RefundSettings>(
    builder.Configuration.GetSection("RefundSettings"));

builder.Services.AddScoped<IFraudFlagRepository, FraudFlagRepository>();
builder.Services.AddScoped<IFraudService, FraudService>();

builder.Services.AddScoped<IRefundRepository, RefundRepository>();

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Logging.AddDebug();
var app = builder.Build();

// Auto migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

GlobalJobFilters.Filters.Add(
    app.Services.GetRequiredService<JobFailureTimelineFilter>()
);

// Seed
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}
// Exception
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();

app.UseSession();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseCookiePolicy();

app.MapControllers();

app.MapHub<FraudHub>("/fraudHub");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[]
    {
        new HangfireDashboardAuthorizationFilter()
    }
});
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapGet("/_routes", (IEnumerable<EndpointDataSource> sources) =>
{
    var routes = sources
        .SelectMany(s => s.Endpoints)
        .OfType<RouteEndpoint>()
        .Select(e => new
        {
            Route = e.RoutePattern.RawText,
            DisplayName = e.DisplayName
        });

    return Results.Json(routes);
});

app.UseHangfireDashboard("/hangfire");
app.MapHub<FraudAlertHub>("/fraudAlertHub");

using (var scope = app.Services.CreateScope())
{
    var recurring = scope.ServiceProvider.GetRequiredService<Hangfire.IRecurringJobManager>();
    recurring.AddOrUpdate<OutboxProcessorJob>(
        "process-outbox",
        job => job.ProcessAsync(),
        "*/1 * * * *");
}

app.Run();