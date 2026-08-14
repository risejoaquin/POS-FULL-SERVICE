using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PosApplication.Interfaces.Server;

namespace PosInfrastructure.Data.Server;

/// <summary>
/// MACROFASE 12C design-time factory for InitialProductionBaseline generation.
/// Keeps EF Core tooling away from PosServer/Program.cs startup validation so migration generation
/// can run without executing JWT startup validation and without requiring JWT_KEY, JWT_ISSUER or JWT_AUDIENCE.
/// It also resolves the CentralDbContext constructor ambiguity seen by dotnet ef.
/// </summary>
public sealed class CentralDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CentralDbContext>
{
    private const string DesignTimeFallbackConnectionString =
        "Host=localhost;Port=5432;Database=pos_design_time;Username=postgres;Password=postgres;SSL Mode=Disable";

    public CentralDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var connectionString = ResolveConnectionString();
        var optionsBuilder = new DbContextOptionsBuilder<CentralDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.CommandTimeout(120);
            options.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
        });

        return new CentralDbContext(optionsBuilder.Options, new DesignTimeTenantContext());
    }

    private static string ResolveConnectionString()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("MACROFASE12_DESIGNTIME_DATABASE_URL")
            ?? DesignTimeFallbackConnectionString;

        if (connectionString.StartsWith('"') && connectionString.EndsWith('"'))
        {
            connectionString = connectionString.Trim('"');
        }

        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = ConvertPostgresUrlToNpgsqlConnectionString(connectionString);
        }

        if ((connectionString.Contains("supabase.com", StringComparison.OrdinalIgnoreCase)
             || connectionString.Contains("pooler", StringComparison.OrdinalIgnoreCase))
            && !connectionString.Contains("Max Auto Prepare", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += ";Max Auto Prepare=0;Pooling=false;";
        }

        return connectionString;
    }

    private static string ConvertPostgresUrlToNpgsqlConnectionString(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = WebUtility.UrlDecode(userInfo[0]);
        var password = userInfo.Length > 1 ? WebUtility.UrlDecode(userInfo[1]) : string.Empty;
        var database = uri.LocalPath.TrimStart('/');
        var port = uri.IsDefaultPort ? 5432 : uri.Port;

        return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=True";
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        private string _tenantId = "design-time-tenant";
        private string _username = "design-time-user";
        private string _userId = "design-time-user-id";

        public void SetTenantId(string tenantId) => _tenantId = tenantId;
        public string GetTenantId() => _tenantId;
        public void SetUsername(string username) => _username = username;
        public string GetUsername() => _username;
        public string GetUserId() => _userId;
        public void SetUserId(string userId) => _userId = userId;
    }
}
