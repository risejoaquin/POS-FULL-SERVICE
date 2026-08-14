using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PosApplication.Interfaces.Server;
using PosDomain.Entities;
using PosInfrastructure.Data.Server;
using Xunit;

namespace PosServer.Tests;

public class TenantIsolationTests
{
    private (CentralDbContext DbContext, Mock<ITenantContext> TenantContextMock) CreateInMemoryDbContext(string tenantId, string username = "testuser")
    {
        var options = new DbContextOptionsBuilder<CentralDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContextMock = new Mock<ITenantContext>();
        tenantContextMock.Setup(t => t.GetTenantId()).Returns(tenantId);
        tenantContextMock.Setup(t => t.GetUsername()).Returns(username);

        var dbContext = new CentralDbContext(options, tenantContextMock.Object);
        return (dbContext, tenantContextMock);
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldFilterEntities_ByCurrentTenantId()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<CentralDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var mockContextA = new Mock<ITenantContext>();
        mockContextA.Setup(t => t.GetTenantId()).Returns("tenant-a");
        var dbContextA = new CentralDbContext(options, mockContextA.Object);

        var mockContextB = new Mock<ITenantContext>();
        mockContextB.Setup(t => t.GetTenantId()).Returns("tenant-b");
        var dbContextB = new CentralDbContext(options, mockContextB.Object);

        // Act - tenant A adds a product
        var productA = new Product { Name = "Tenant A Product", Barcode = "A100", Price = 10m };
        dbContextA.Products.Add(productA);
        await dbContextA.SaveChangesAsync();

        // tenant B adds a product
        var productB = new Product { Name = "Tenant B Product", Barcode = "B200", Price = 20m };
        dbContextB.Products.Add(productB);
        await dbContextB.SaveChangesAsync();

        // Assert - tenant A should only see their own product
        var productsForA = await dbContextA.Products.ToListAsync();
        Assert.Single(productsForA);
        Assert.Equal("Tenant A Product", productsForA[0].Name);
        Assert.Equal("tenant-a", productsForA[0].TenantId);

        // tenant B should only see their own product
        var productsForB = await dbContextB.Products.ToListAsync();
        Assert.Single(productsForB);
        Assert.Equal("Tenant B Product", productsForB[0].Name);
        Assert.Equal("tenant-b", productsForB[0].TenantId);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPreventModifyingEntity_OfAnotherTenant()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<CentralDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var mockContextA = new Mock<ITenantContext>();
        mockContextA.Setup(t => t.GetTenantId()).Returns("tenant-a");
        var dbContextA = new CentralDbContext(options, mockContextA.Object);

        var mockContextB = new Mock<ITenantContext>();
        mockContextB.Setup(t => t.GetTenantId()).Returns("tenant-b");
        var dbContextB = new CentralDbContext(options, mockContextB.Object);

        // Add a product under tenant A
        var productA = new Product { Name = "Tenant A Product", Barcode = "A100", Price = 10m };
        dbContextA.Products.Add(productA);
        await dbContextA.SaveChangesAsync();

        // Get product A reference inside tenant B's bypass or raw tracking (simulating an escalation)
        var untrackedProduct = new Product 
        { 
            Id = productA.Id, 
            TenantId = "tenant-a", // belongs to tenant a
            Name = "Attacked Product",
            Barcode = "A100",
            Price = 15m 
        };

        dbContextB.Entry(untrackedProduct).State = EntityState.Modified;

        // Act & Assert - should throw UnauthorizedAccessException when tenant B tries to save modifications
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await dbContextB.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldAutomaticallyGenerate_AuditLogsAndOutboxMessages()
    {
        // Arrange
        var (dbContext, _) = CreateInMemoryDbContext("tenant-a", "test-user");

        // Act
        var product = new Product { Name = "New Product", Barcode = "NP100", Price = 15m };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        // Assert - check that AuditLogs contains an Added event for Product
        var auditLogs = await dbContext.AuditLogs.IgnoreQueryFilters().ToListAsync();
        Assert.NotEmpty(auditLogs);
        var productAudit = auditLogs.FirstOrDefault(l => l.EntityType == "Product" && l.Action == "Added");
        Assert.NotNull(productAudit);
        Assert.Equal("test-user", productAudit.UserId);
        Assert.Equal("tenant-a", productAudit.TenantId);

        // Assert - check that OutboxMessages contains a Product_Added event
        var outboxMessages = await dbContext.OutboxMessages.IgnoreQueryFilters().ToListAsync();
        Assert.NotEmpty(outboxMessages);
        var productOutbox = outboxMessages.FirstOrDefault(m => m.EventType == "Product_Added");
        Assert.NotNull(productOutbox);
        Assert.Equal("tenant-a", productOutbox.TenantId);
        Assert.Contains("New Product", productOutbox.Payload);
    }
}
