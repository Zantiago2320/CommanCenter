using CommanCenter.Portal.Models;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using CommanCenter.Portal.Services;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// ── Azure Key Vault ────────────────────────────────────────────────────────
if (builder.Configuration.GetValue<bool>("KeyVault:Enabled"))
{
    var vaultUri = builder.Configuration["KeyVault:VaultUri"]
        ?? throw new InvalidOperationException("KeyVault:VaultUri es obligatorio cuando KeyVault:Enabled = true.");

    builder.Configuration.AddAzureKeyVault(
        new Uri(vaultUri),
        new DefaultAzureCredential());
}

// ── Serilog ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/portal-.log", rollingInterval: Serilog.RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ── Razor Pages ────────────────────────────────────────────────────────────
builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

// ── HttpClient → API ───────────────────────────────────────────────────────
var apiBase = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001/";
var timeout = TimeSpan.FromSeconds(
    builder.Configuration.GetValue<int>("ApiSettings:TimeoutSeconds", 30));

builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.Timeout = timeout;
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("X-Source", "CommanCenter.Portal");
});

// ── Auth Cookies ───────────────────────────────────────────────────────────
var cookieName   = builder.Configuration["Authentication:CookieName"]  ?? "CommanCenter.Auth";
var loginPath    = builder.Configuration["Authentication:LoginPath"]    ?? "/Auth/Login";
var expireMinutes = builder.Configuration.GetValue<int>("Authentication:ExpireMinutes", 480);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name     = cookieName;
        options.LoginPath       = loginPath;
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan  = TimeSpan.FromMinutes(expireMinutes);
    });

builder.Services.AddAuthorization();

// ── Application Services ───────────────────────────────────────────────────
builder.Services.AddScoped<IAuthPortalService, AuthPortalService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();

// ── Notification API ───────────────────────────────────────────────────────
var notifBase    = builder.Configuration["NotificationApi:BaseUrl"] ?? "";
var notifTimeout = TimeSpan.FromSeconds(
    builder.Configuration.GetValue<int>("NotificationApi:TimeoutSeconds", 15));
var notifApiKey  = builder.Configuration["NotificationApi:ApiKey"] ?? "";

builder.Services.AddHttpClient<INotificationApiService, NotificationApiService>(client =>
{
    if (!string.IsNullOrWhiteSpace(notifBase))
        client.BaseAddress = new Uri(notifBase);
    client.Timeout = notifTimeout;
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    if (!string.IsNullOrWhiteSpace(notifApiKey))
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", notifApiKey);
});

// ── Application Insights ───────────────────────────────────────────────────
if (builder.Configuration.GetValue<bool>("ApplicationInsights:Enabled"))
    builder.Services.AddApplicationInsightsTelemetry(options =>
        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]);

// ── Session (para token JWT almacenado en sesión) ──────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(expireMinutes);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSerilogRequestLogging();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

Log.Information("🌐 CommanCenter Portal iniciado");

app.Run();
