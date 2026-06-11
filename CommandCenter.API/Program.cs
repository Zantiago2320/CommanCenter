using CommandCenter.API.Extensions;
using CommandCenter.API.Infrastructure.Data;
using CommandCenter.API.Middleware;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .WriteTo.File("Logs/commandcenter-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ── Servicios ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityConfig();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerConfig(builder.Configuration);
builder.Services.AddHangfireConfig(builder.Configuration);
builder.Services.AddCorsConfig(builder.Configuration);
builder.Services.AddApplicationServices();

// Application Insights (habilitar en producción)
if (builder.Configuration.GetValue<bool>("ApplicationInsights:Enabled"))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    });
}

// ── Build ──────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Migración automática + Seed ─────────────────────────────────────────────
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await db.Database.MigrateAsync();
    await SeedAsync(roleManager, userManager);
}
catch (Exception ex)
{
    Log.Error(ex, "❌ Error durante la migración/seed de base de datos — la app continúa sin BD");
}

// ── Middleware Pipeline ────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Swagger habilitado en todos los entornos para facilitar diagnóstico
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Command Center API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("CommandCenterPolicy");
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// Hangfire Dashboard (solo admins en producción)
app.UseHangfireDashboard(
    builder.Configuration["Hangfire:DashboardPath"] ?? "/jobs",
    new DashboardOptions { Authorization = new[] { new HangfireAuthFilter() } });

app.MapControllers();

Log.Information("🚀 Command Center API iniciada en {Environment}", app.Environment.EnvironmentName);

app.Run();

// ── Seed inicial ───────────────────────────────────────────────────────────
static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
{
    string[] roles = ["SuperAdmin", "Admin", "Lider", "User"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Usuarios iniciales: se inicia sesión con el NOMBRE de usuario (no con correo).
    await CrearUsuarioAsync(userManager, "alexander", "Alexander@123", "SuperAdmin");
    await CrearUsuarioAsync(userManager, "sergio", "Sergio@123", "SuperAdmin");
    await CrearUsuarioAsync(userManager, "lider", "Lider@123", "Lider");
}

// Crea un usuario por nombre (sin correo) y le asigna un rol, si no existe.
static async Task CrearUsuarioAsync(UserManager<IdentityUser> userManager,
    string userName, string password, string rol)
{
    if (await userManager.FindByNameAsync(userName) is not null)
        return;

    var user = new IdentityUser { UserName = userName, EmailConfirmed = true };
    var result = await userManager.CreateAsync(user, password);

    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(user, rol);
        Log.Warning("✅ Usuario creado: {Usuario} [{Rol}] — Cambiar contraseña en producción", userName, rol);
    }
    else
    {
        Log.Error("❌ No se pudo crear el usuario {Usuario}: {Errores}",
            userName, string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
