using System;
using Microsoft.EntityFrameworkCore;
using PosServer.Data;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static void Main()
    {
        var services = new ServiceCollection();
        services.AddDbContext<CentralDbContext>(options => options.UseNpgsql("Host=localhost;Database=test;Username=postgres;Password=postgres"));
        services.AddSingleton<PosServer.Services.ITenantService, DummyTenantService>();
        var provider = services.BuildServiceProvider();
        var ctx = provider.GetRequiredService<CentralDbContext>();
        var script = ctx.Database.GenerateCreateScript();
        script = script.Replace("CREATE TABLE", "CREATE TABLE IF NOT EXISTS")
                       .Replace("CREATE INDEX", "CREATE INDEX IF NOT EXISTS")
                       .Replace("CREATE UNIQUE INDEX", "CREATE UNIQUE INDEX IF NOT EXISTS");
        Console.WriteLine(script);
    }
}

class DummyTenantService : PosServer.Services.ITenantService
{
    public string GetTenantId() => "test";
}
