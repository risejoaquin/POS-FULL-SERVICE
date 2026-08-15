using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using PosApplication.Interfaces.Server;
using PosDomain.Entities;
using PosInfrastructure.Data.Local;
using PosInfrastructure.Services.Local;
using Xunit;

namespace PosInfrastructure.Tests;

public class AuthCredentialStorageTests
{
    private const string TenantId = "tenant-1";

    [Fact]
    public async Task CreateUserAsync_Should_Store_BCrypt_Hash_And_Not_Plain_Pin()
    {
        await using var database = await CreateDatabaseAsync();
        var service = new UsersService(database.Context);

        var user = await service.CreateUserAsync("cashier", "123456", "Cashier");

        Assert.Null(user.Pin);
        Assert.NotEqual("123456", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("123456", user.PasswordHash));
    }

    [Fact]
    public async Task ResetPinAsync_Should_Replace_With_BCrypt_Hash()
    {
        await using var database = await CreateDatabaseAsync();
        var service = new UsersService(database.Context);
        var user = await service.CreateUserAsync("cashier", "123456", "Cashier");

        await service.ResetPinAsync(user.Id, "654321");
        var updated = await database.Context.Users.SingleAsync(u => u.Id == user.Id);

        Assert.Null(updated.Pin);
        Assert.NotEqual("654321", updated.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("654321", updated.PasswordHash));
        Assert.False(BCrypt.Net.BCrypt.Verify("123456", updated.PasswordHash));
    }

    [Fact]
    public async Task CacheCloudLoginAsync_Should_Store_Hash_And_Allow_Local_Login()
    {
        await using var database = await CreateDatabaseAsync();
        var authService = new LocalAuthService(database.Context);

        var cached = await authService.CacheCloudLoginAsync("admin", "strong-pin", TenantId, "Admin");
        var stored = await database.Context.Users.SingleAsync(u => u.Username == "admin");
        var localLogin = await authService.AuthenticateLocalUserAsync("admin", "strong-pin");

        Assert.True(cached.IsSuccess);
        Assert.True(localLogin.IsSuccess);
        Assert.Null(stored.Pin);
        Assert.NotEqual("strong-pin", stored.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("strong-pin", stored.PasswordHash));
    }

    [Fact]
    public async Task EmptyDatabase_Should_Not_Allow_Admin_Admin_Backdoor()
    {
        await using var database = await CreateDatabaseAsync();
        var authService = new LocalAuthService(database.Context);

        var result = await authService.AuthenticateLocalUserAsync("admin", "admin");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ManagerOverride_Should_Not_Accept_Admin_Backdoor()
    {
        await using var database = await CreateDatabaseAsync();
        var authService = new LocalAuthService(database.Context);

        var result = await authService.ValidateManagerOverrideAsync(string.Empty, "admin");

        Assert.False(result);
    }

    private static async Task<TestDatabase> CreateDatabaseAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        return new TestDatabase(connection, context);
    }

    private static PosDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseSqlite(connection)
            .Options;

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.Setup(t => t.GetTenantId()).Returns(TenantId);
        return new PosDbContext(options, tenantContext.Object);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        public TestDatabase(SqliteConnection connection, PosDbContext context)
        {
            Connection = connection;
            Context = context;
        }

        public SqliteConnection Connection { get; }
        public PosDbContext Context { get; }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
