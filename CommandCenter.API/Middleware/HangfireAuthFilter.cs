using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace CommandCenter.API.Middleware;

/// <summary>
/// Filtro de autorización para el Dashboard de Hangfire.
/// En producción valida que el usuario sea SuperAdmin.
/// </summary>
public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // En desarrollo permite acceso libre
        if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            return true;

        // En producción requiere autenticación y rol SuperAdmin
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("SuperAdmin");
    }
}
