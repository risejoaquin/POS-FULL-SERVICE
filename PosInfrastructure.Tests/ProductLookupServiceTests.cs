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

namespace PosInfrastructure.Tests
{
    public class ProductLookupServiceTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly PosDbContext _dbContext;
        private readonly ProductLookupService _service;

        public ProductLookupServiceTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<PosDbContext>()
                .UseSqlite(_connection)
                .Options;

            var mockTenant = new Mock<ITenantContext>();
            mockTenant.Setup(t => t.GetTenantId()).Returns("test-tenant");

            _dbContext = new PosDbContext(options, mockTenant.Object);
            _dbContext.Database.EnsureCreated();

            _service = new ProductLookupService(_dbContext);
        }

        private async Task SeedProductAsync(
            int id,
            string name,
            string barcode,
            decimal price,
            int stockQuantity,
            string tenantId = "test-tenant")
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO Products
                (
                    Id,
                    Name,
                    Barcode,
                    Price,
                    StockQuantity,
                    MinStockThreshold,
                    Category,
                    ImagePath,
                    IsActive,
                    RowVersion,
                    LastUpdated,
                    TenantId,
                    CustomAttributes
                )
                VALUES
                (
                    {0},
                    {1},
                    {2},
                    {3},
                    {4},
                    {5},
                    {6},
                    {7},
                    {8},
                    {9},
                    {10},
                    {11},
                    {12}
                )
                """,
                id,
                name,
                barcode,
                price,
                stockQuantity,
                10,
                "General",
                "",
                true,
                1,
                DateTime.UtcNow,
                tenantId,
                "{}");
        }

        [Fact]
        public async Task LookupByBarcodeAsync_Should_ReturnProduct_When_Exists()
        {
            // Arrange
            await SeedProductAsync(1, "Gansito", "7501000111201", 15.5m, 10);

            // Act
            var result = await _service.LookupByBarcodeAsync("7501000111201");

            // Assert
            Assert.True(result.Found);
            Assert.NotNull(result.Product);
            Assert.Equal("Gansito", result.Product.Name);
        }

        [Fact]
        public async Task LookupByBarcodeAsync_Should_ReturnFalse_When_DoesNotExist()
        {
            // Act
            var result = await _service.LookupByBarcodeAsync("9999999999999");

            // Assert
            Assert.False(result.Found);
            Assert.Null(result.Product);
        }

        [Fact]
        public async Task LookupByIdAsync_Should_ReturnProduct_When_Exists()
        {
            // Arrange
            await SeedProductAsync(2, "Coca-Cola", "7501055300012", 18.0m, 5);

            // Act
            var result = await _service.LookupByIdAsync(2);

            // Assert
            Assert.True(result.Found);
            Assert.NotNull(result.Product);
            Assert.Equal("Coca-Cola", result.Product.Name);
        }

        [Fact]
        public async Task LookupByIdAsync_Should_ReturnFalse_When_DoesNotExist()
        {
            // Act
            var result = await _service.LookupByIdAsync(999);

            // Assert
            Assert.False(result.Found);
            Assert.Null(result.Product);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            _connection.Close();
            _connection.Dispose();
        }
    }
}
