namespace PosServer.Middlewares;

using Microsoft.AspNetCore.Http;
using PosApplication.Interfaces.Server;
using Microsoft.EntityFrameworkCore;
using PosInfrastructure.Data.Server;
using System.Threading.Tasks;

public class PostgresTenantMiddleware
{
    private readonly RequestDelegate _next;

    public PostgresTenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, CentralDbContext dbContext)
    {
        var tenantId = tenantContext.GetTenantId();
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            // Establish the tenant context for PostgreSQL Row Level Security
            // We use set_config to set a session variable that RLS policies will read
            // The third parameter 'false' means it applies to the current session/transaction
            await dbContext.Database.ExecuteSqlAsync($"SELECT set_config('app.current_tenant', {tenantId}, false);");
        }

        await _next(context);
    }
}
