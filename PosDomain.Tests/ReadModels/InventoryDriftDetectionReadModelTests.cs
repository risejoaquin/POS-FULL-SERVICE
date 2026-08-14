using System;
using System.Collections.Generic;
using System.Linq;
using PosDomain.Entities;
using PosDomain.ReadModels;
using Xunit;

namespace PosDomain.Tests.ReadModels;

public class InventoryDriftDetectionReadModelTests
{
    [Fact]
    public void DetectProductDrift_WhenOperationalMatchesLedger_ReturnsNoDrift()
    {
        var movements = new[]
        {
            InventoryMovement.ProductSale(1, 2m, "tenant-1", "order-1"),
            InventoryMovement.ProductRestock(1, 5m, "tenant-1", "restock-1")
        };
        var opening = new Dictionary<int, decimal> { [1] = 10m };
        var operational = new Dictionary<int, decimal> { [1] = 13m };

        var report = InventoryDriftDetectionReadModel.DetectProductDrift(movements, operational, opening, "tenant-1");

        Assert.False(report.HasDrift);
        Assert.Equal(1, report.TotalItems);
        Assert.Equal(0, report.DriftedItemCount);
        Assert.Equal(13m, report.Items.Single().LedgerQuantity);
        Assert.Equal(0m, report.Items.Single().DriftQuantity);
    }

    [Fact]
    public void DetectProductDrift_WhenOperationalDiffersFromLedger_ReturnsDrift()
    {
        var movements = new[]
        {
            InventoryMovement.ProductSale(1, 2m, "tenant-1", "order-1")
        };
        var opening = new Dictionary<int, decimal> { [1] = 10m };
        var operational = new Dictionary<int, decimal> { [1] = 9m };

        var report = InventoryDriftDetectionReadModel.DetectProductDrift(movements, operational, opening, "tenant-1");
        var item = report.Items.Single();

        Assert.True(report.HasDrift);
        Assert.Equal(1, report.DriftedItemCount);
        Assert.Equal(8m, item.LedgerQuantity);
        Assert.Equal(9m, item.OperationalQuantity);
        Assert.Equal(1m, item.DriftQuantity);
        Assert.True(item.HasDrift);
    }

    [Fact]
    public void DetectProductDrift_UsesSignedQuantityForLegacyNegativeMovements()
    {
        var movements = new[]
        {
            new InventoryMovement
            {
                ProductId = 1,
                Quantity = -2m,
                MovementType = InventoryMovement.SaleType,
                TenantId = "tenant-1"
            }
        };
        var opening = new Dictionary<int, decimal> { [1] = 10m };
        var operational = new Dictionary<int, decimal> { [1] = 8m };

        var report = InventoryDriftDetectionReadModel.DetectProductDrift(movements, operational, opening, "tenant-1");
        var item = report.Items.Single();

        Assert.False(report.HasDrift);
        Assert.Equal(8m, item.LedgerQuantity);
        Assert.True(item.HasLegacyNegativeMovement);
    }

    [Fact]
    public void DetectSupplyDrift_UsesRecipeConsumptionAsLedgerDecrease()
    {
        var movements = new[]
        {
            InventoryMovement.SupplyConsumption(7, 1.5m, "tenant-1", "recipe-1"),
            new InventoryMovement
            {
                SupplyId = 7,
                Quantity = 3m,
                MovementType = InventoryMovement.RestockType,
                TenantId = "tenant-1"
            }
        };
        var opening = new Dictionary<int, decimal> { [7] = 5m };
        var operational = new Dictionary<int, decimal> { [7] = 6.5m };

        var report = InventoryDriftDetectionReadModel.DetectSupplyDrift(movements, operational, opening, "tenant-1");
        var item = report.Items.Single();

        Assert.False(report.HasDrift);
        Assert.Equal(InventoryLedgerReadModel.SupplyEntityType, item.EntityType);
        Assert.Equal(1.5m, item.MovementDelta);
        Assert.Equal(6.5m, item.LedgerQuantity);
    }

    [Fact]
    public void DetectProductDrift_WhenTenantIsProvided_FiltersMovementsByTenant()
    {
        var movements = new[]
        {
            InventoryMovement.ProductSale(1, 2m, "tenant-1", "order-1"),
            InventoryMovement.ProductSale(1, 8m, "tenant-2", "order-2")
        };
        var opening = new Dictionary<int, decimal> { [1] = 10m };
        var operational = new Dictionary<int, decimal> { [1] = 8m };

        var report = InventoryDriftDetectionReadModel.DetectProductDrift(movements, operational, opening, "tenant-1");

        Assert.False(report.HasDrift);
        Assert.Equal(8m, report.Items.Single().LedgerQuantity);
        Assert.Equal(1, report.Items.Single().MovementCount);
    }

    [Fact]
    public void DetectProductDrift_IncludesOperationalItemsWithoutMovements()
    {
        var movements = Array.Empty<InventoryMovement>();
        var operational = new Dictionary<int, decimal> { [3] = 4m };

        var report = InventoryDriftDetectionReadModel.DetectProductDrift(movements, operational);
        var item = report.Items.Single();

        Assert.True(report.HasDrift);
        Assert.Equal(3, item.EntityId);
        Assert.Equal(0m, item.LedgerQuantity);
        Assert.Equal(4m, item.OperationalQuantity);
        Assert.Equal(4m, item.DriftQuantity);
    }

    [Fact]
    public void DetectProductDrift_IncludesLedgerItemsMissingFromOperationalStock()
    {
        var movements = new[]
        {
            InventoryMovement.ProductRestock(9, 6m, "tenant-1", "restock-1")
        };
        var operational = new Dictionary<int, decimal>();

        var report = InventoryDriftDetectionReadModel.DetectProductDrift(movements, operational, tenantId: "tenant-1");
        var item = report.Items.Single();

        Assert.True(report.HasDrift);
        Assert.Equal(9, item.EntityId);
        Assert.Equal(6m, item.LedgerQuantity);
        Assert.Equal(0m, item.OperationalQuantity);
        Assert.Equal(-6m, item.DriftQuantity);
    }

    [Fact]
    public void DetectProductDrift_ExposesNegativeLedgerItems()
    {
        var movements = new[]
        {
            InventoryMovement.ProductSale(1, 12m, "tenant-1", "order-1")
        };
        var opening = new Dictionary<int, decimal> { [1] = 10m };
        var operational = new Dictionary<int, decimal> { [1] = 0m };

        var report = InventoryDriftDetectionReadModel.DetectProductDrift(movements, operational, opening, "tenant-1");
        var negative = report.NegativeLedgerItems.Single();

        Assert.True(negative.IsNegativeLedgerBalance);
        Assert.Equal(-2m, negative.LedgerQuantity);
    }

    [Fact]
    public void DetectProductDrift_WhenMovementIsInvalid_ThrowsFromLedgerValidation()
    {
        var movements = new[]
        {
            new InventoryMovement
            {
                ProductId = 1,
                Quantity = 0m,
                MovementType = InventoryMovement.SaleType,
                TenantId = "tenant-1"
            }
        };
        var operational = new Dictionary<int, decimal> { [1] = 0m };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            InventoryDriftDetectionReadModel.DetectProductDrift(movements, operational, tenantId: "tenant-1"));

        Assert.Equal("Quantity must be different from zero.", exception.Message);
    }

    [Fact]
    public void DetectProductDrift_WhenArgumentsAreNull_Throws()
    {
        var operational = new Dictionary<int, decimal>();

        Assert.Throws<ArgumentNullException>(() =>
            InventoryDriftDetectionReadModel.DetectProductDrift(null!, operational));

        Assert.Throws<ArgumentNullException>(() =>
            InventoryDriftDetectionReadModel.DetectProductDrift(Array.Empty<InventoryMovement>(), null!));
    }
}
