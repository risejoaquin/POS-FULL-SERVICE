using System;
using System.Collections.Generic;
using PosDomain.Entities;
using PosDomain.ReadModels;
using Xunit;

namespace PosDomain.Tests.ReadModels;

public class InventoryLedgerReadModelTests
{
    [Fact]
    public void CalculateProductBalance_UsesSignedQuantityForCanonicalPositiveMovements()
    {
        var movements = new[]
        {
            InventoryMovement.ProductSale(1, 2m, "tenant-1", "order-1"),
            InventoryMovement.ProductReturn(1, 1m, "tenant-1", "return-1"),
            InventoryMovement.ProductRestock(1, 5m, "tenant-1", "restock-1")
        };

        var balance = InventoryLedgerReadModel.CalculateProductBalance(movements, productId: 1, openingQuantity: 10m);

        Assert.Equal(1, balance.EntityId);
        Assert.Equal(InventoryLedgerReadModel.ProductEntityType, balance.EntityType);
        Assert.Equal(10m, balance.OpeningQuantity);
        Assert.Equal(4m, balance.MovementDelta);
        Assert.Equal(14m, balance.CurrentQuantity);
        Assert.Equal(3, balance.MovementCount);
        Assert.False(balance.HasLegacyNegativeMovement);
    }

    [Fact]
    public void CalculateProductBalance_HandlesLegacyNegativeSaleWithoutDoubleNegating()
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

        var balance = InventoryLedgerReadModel.CalculateProductBalance(movements, productId: 1, openingQuantity: 10m);

        Assert.Equal(-2m, balance.MovementDelta);
        Assert.Equal(8m, balance.CurrentQuantity);
        Assert.True(balance.HasLegacyNegativeMovement);
    }

    [Fact]
    public void CalculateProductBalance_IgnoresOtherProductsAndSupplyMovements()
    {
        var movements = new[]
        {
            InventoryMovement.ProductSale(1, 2m, "tenant-1", "order-1"),
            InventoryMovement.ProductSale(2, 9m, "tenant-1", "order-2"),
            InventoryMovement.SupplyConsumption(7, 4m, "tenant-1", "recipe-1")
        };

        var balance = InventoryLedgerReadModel.CalculateProductBalance(movements, productId: 1, openingQuantity: 20m);

        Assert.Equal(-2m, balance.MovementDelta);
        Assert.Equal(18m, balance.CurrentQuantity);
        Assert.Equal(1, balance.MovementCount);
    }

    [Fact]
    public void CalculateSupplyBalance_UsesRecipeConsumptionAsDecrease()
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

        var balance = InventoryLedgerReadModel.CalculateSupplyBalance(movements, supplyId: 7, openingQuantity: 5m);

        Assert.Equal(7, balance.EntityId);
        Assert.Equal(InventoryLedgerReadModel.SupplyEntityType, balance.EntityType);
        Assert.Equal(1.5m, balance.MovementDelta);
        Assert.Equal(6.5m, balance.CurrentQuantity);
    }

    [Fact]
    public void CalculateProductBalance_WhenTenantIsProvided_FiltersByTenant()
    {
        var movements = new[]
        {
            InventoryMovement.ProductSale(1, 2m, "tenant-1", "order-1"),
            InventoryMovement.ProductSale(1, 8m, "tenant-2", "order-2")
        };

        var balance = InventoryLedgerReadModel.CalculateProductBalance(movements, productId: 1, openingQuantity: 10m, tenantId: "tenant-1");

        Assert.Equal(-2m, balance.MovementDelta);
        Assert.Equal(8m, balance.CurrentQuantity);
        Assert.Equal(1, balance.MovementCount);
    }

    [Fact]
    public void BuildProductBalances_GroupsMovementsByProduct()
    {
        var movements = new[]
        {
            InventoryMovement.ProductSale(1, 2m, "tenant-1", "order-1"),
            InventoryMovement.ProductRestock(1, 5m, "tenant-1", "restock-1"),
            InventoryMovement.ProductSale(2, 1m, "tenant-1", "order-2")
        };
        var opening = new Dictionary<int, decimal>
        {
            [1] = 10m,
            [2] = 4m
        };

        var balances = InventoryLedgerReadModel.BuildProductBalances(movements, opening, "tenant-1");

        Assert.Equal(13m, balances[1].CurrentQuantity);
        Assert.Equal(3m, balances[2].CurrentQuantity);
    }

    [Fact]
    public void BuildSupplyBalances_GroupsMovementsBySupply()
    {
        var movements = new[]
        {
            InventoryMovement.SupplyConsumption(7, 0.5m, "tenant-1", "recipe-1"),
            InventoryMovement.SupplyConsumption(8, 2m, "tenant-1", "recipe-2"),
            new InventoryMovement
            {
                SupplyId = 7,
                Quantity = 4m,
                MovementType = InventoryMovement.RestockType,
                TenantId = "tenant-1"
            }
        };
        var opening = new Dictionary<int, decimal>
        {
            [7] = 1m,
            [8] = 10m
        };

        var balances = InventoryLedgerReadModel.BuildSupplyBalances(movements, opening, "tenant-1");

        Assert.Equal(4.5m, balances[7].CurrentQuantity);
        Assert.Equal(8m, balances[8].CurrentQuantity);
    }

    [Fact]
    public void CalculateProductBalance_WhenInterpretedMovementIsInvalid_Throws()
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

        var exception = Assert.Throws<InvalidOperationException>(() =>
            InventoryLedgerReadModel.CalculateProductBalance(movements, productId: 1));

        Assert.Equal("Quantity must be different from zero.", exception.Message);
    }

    [Fact]
    public void CalculateProductBalance_CanExposeNegativeReconstructedBalanceWithoutMutatingStock()
    {
        var movements = new[]
        {
            InventoryMovement.ProductSale(1, 12m, "tenant-1", "order-1")
        };

        var balance = InventoryLedgerReadModel.CalculateProductBalance(movements, productId: 1, openingQuantity: 10m);

        Assert.Equal(-2m, balance.CurrentQuantity);
        Assert.True(balance.IsNegative);
    }

    [Fact]
    public void CalculateProductBalance_WhenMovementsIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            InventoryLedgerReadModel.CalculateProductBalance(null!, productId: 1));
    }
}
