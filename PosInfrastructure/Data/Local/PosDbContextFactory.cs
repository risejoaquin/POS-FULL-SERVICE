using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PosApplication.Interfaces.Server;
using Microsoft.Extensions.Configuration;
using PosDomain.Interfaces;

namespace PosInfrastructure.Data.Local;

public class PosDbContextFactory : IDesignTimeDbContextFactory<PosDbContext>
{
    public PosDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<PosDbContext>();
        var connectionString = configuration.GetSection("DatabaseSettings")["ConnectionString"];
        builder.UseSqlite(connectionString);

        var sessionManager = new DesignTimeTenantContext();
        sessionManager.SetTenantId("DESIGN_TIME_DEFAULT_TENANT"); // Default for design time

        return new PosDbContext(builder.Options, sessionManager);
    }
}

public class DesignTimeTenantContext : ITenantContext
{
    private string _tenantId = string.Empty;
    public void SetTenantId(string tenantId) => _tenantId = tenantId;
    public string GetTenantId() => _tenantId;
    public void SetUsername(string username) { }
    public string GetUsername() => string.Empty;
    public void SetUserId(string userId) { }
    public string GetUserId() => string.Empty;
}
