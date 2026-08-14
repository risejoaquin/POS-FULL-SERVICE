namespace PosServer.Middlewares;

using Microsoft.AspNetCore.Http;
using PosApplication.Interfaces.Server;
using PosDomain.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantId = context.User?.FindFirstValue("TenantId");
        var username = context.User?.Claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        var path = context.Request.Path.Value?.ToLower() ?? "";
        bool isExemptRoute = path == "/" ||
                             path == "/health" ||
                             path == "/api/health" ||
                             path.StartsWith("/health/") ||
                             path == "/metrics" ||
                             path.Contains("/swagger") ||
                             path.Contains("/api/license/validate") ||
                             path.Contains("/api/license/generate") ||
                             path.StartsWith("/api/auth/login") ||
                             path.StartsWith("/api/v1/auth/login") ||
                             path.StartsWith("/api/auth/provision") ||
                             path.StartsWith("/api/v1/auth/provision");

        if (string.IsNullOrEmpty(tenantId))
        {
            // Only allow X-Tenant-Id for unauthenticated requests on specific routes (e.g., provisioning/login)
            if (string.IsNullOrEmpty(username))
            {
                 tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? string.Empty;
            }
        }

        if (string.IsNullOrEmpty(tenantId) && !isExemptRoute)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { Error = "TenantId no puede ser determinado a partir del token o los headers válidos." });
            return;
        }

        if (!string.IsNullOrEmpty(tenantId))
        {
            tenantContext.SetTenantId(tenantId);
        }
        
        if (!string.IsNullOrEmpty(username))
        {
            tenantContext.SetUsername(username);
            var userId = context.User?.FindFirstValue("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                tenantContext.SetUserId(userId);
            }
        }

        await _next(context);
    }
}
