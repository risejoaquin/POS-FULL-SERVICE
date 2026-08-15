using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using PosApplication.Interfaces.Server;
using PosDomain.Entities;
using PosDomain.ReadModels;
using PosInfrastructure.Data.Local;
using PosInfrastructure.Services.Local;
using Xunit;

namespace PosInfrastructure.Tests;

public class InventoryConsistencyTests
{
    private const string TenantId = "tenant-1";

    [Fact]
    public async Task RegisterSaleAsync_Should_Decrease_Stock_And_Write_Ledger_Movement()
    {
        await using var database = await CreateDatabaseAsync();
        await SeedProductAsync(database.Context, productId: 1, stockQuantity: 5);

        var service = new InventoryService(database.Context);

        await service.RegisterSaleAsync(1, 2, "order-1", TenantId);
        await database.Context.SaveChangesAsync();

        var product = await database.Context.Products.SingleAsync(p => p.Id == 1);
        var movement = await database.Context.InventoryMovements.SingleAsync(m => m.ProductId == 1);

        Assert.Equal(3, product.StockQuantity);
        Assert.Equal(InventoryMovement.SaleType, movement.MovementType);
        Assert.Equal(2, movement.Quantity);
        Assert.Equal(-2, movement.SignedQuantity);
    }

    [Fact]
    public async Task RegisterReturnAsync_Should_Increase_Stock_And_Write_Ledger_Movement()
    {
        await using var database = await CreateDatabaseAsync();
        await SeedProductAsync(database.Context, productId: 1, stockQuantity: 3);

        var service = new InventoryService(database.Context);

        await service.RegisterReturnAsync(1, 2, "return-1", TenantId);
        await database.Context.SaveChangesAsync();

        var product = await database.Context.Products.SingleAsync(p => p.Id == 1);
        var movement = await database.Context.InventoryMovements.SingleAsync(m => m.ProductId == 1);

        Assert.Equal(5, product.StockQuantity);
        Assert.Equal(InventoryMovement.ReturnType, movement.MovementType);
        Assert.Equal(2, movement.Quantity);
        Assert.Equal(2, movement.SignedQuantity);
    }

    [Fact]
    public async Task Ledger_Reconciliation_Should_Match_Product_Read_Model()
    {
        await using var database = await CreateDatabaseAsync();
        await SeedProductAsync(database.Context, productId: 1, stockQuantity: 5);

        var service = new InventoryService(database.Context);
        await service.RegisterSaleAsync(1, 2, "order-1", TenantId);
        await service.RegisterReturnAsync(1, 1, "return-1", TenantId);
        await database.Context.SaveChangesAsync();

        var product = await database.Context.Products.SingleAsync(p => p.Id == 1);
        var movements = await database.Context.InventoryMovements.ToListAsync();
        var openingQuantities = new Dictionary<int, decimal> { [1] = 5m };
        var operationalQuantities = new Dictionary<int, decimal> { [1] = product.StockQuantity };

        var balance = InventoryLedgerReadModel.CalculateProductBalance(movements, 1, openingQuantity: 5m, TenantId);
        var drift = InventoryDriftDetectionReadModel.DetectProductDrift(movements, operationalQuantities, openingQuantities, TenantId);

        Assert.Equal(4, product.StockQuantity);
        Assert.Equal(4m, balance.CurrentQuantity);
        Assert.False(drift.HasDrift);
    }

    [Fact]
    public async Task Concurrent_Double_Sale_Should_Not_Persist_Negative_Stock()
    {
        await using var database = await CreateDatabaseAsync();
        await SeedProductAsync(database.Context, productId: 1, stockQuantity: 1);

        await using var firstContext = CreateContext(database.Connection);
        await using var secondContext = CreateContext(database.Connection);
        var firstService = new InventoryService(firstContext);
        var secondService = new InventoryService(secondContext);

        await firstService.RegisterSaleAsync(1, 1, "order-1", TenantId);
        await secondService.RegisterSaleAsync(1, 1, "order-2", TenantId);

        await firstContext.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync());

        await using var verificationContext = CreateContext(database.Connection);
        var product = await verificationContext.Products.SingleAsync(p => p.Id == 1);
        var movements = await verificationContext.InventoryMovements.Where(m => m.ProductId == 1).ToListAsync();

        Assert.Equal(0, product.StockQuantity);
        Assert.Single(movements);
        Assert.Equal("order-1", movements[0].Reference);
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

    private static async Task SeedProductAsync(PosDbContext context, int productId, int stockQuantity)
    {
        context.Products.Add(new Product
        {
            Id = productId,
            Name = $"Product {productId}",
            Barcode = $"barcode-{productId}",
            Price = 10m,
            StockQuantity = stockQuantity,
            MinStockThreshold = 1,
            Category = "General",
            TenantId = TenantId,
            LastUpdated = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
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
