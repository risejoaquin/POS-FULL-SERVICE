using PosDomain.Entities;
using Xunit;

namespace PosDomain.Tests.Entities;

public class InventoryMovementTests
{
    [Fact]
    public void ProductSale_CreatesDecreasingProductMovement()
    {
        var movement = InventoryMovement.ProductSale(1, 2m, "tenant-1", "order-1");

        Assert.True(movement.IsProductMovement);
        Assert.True(movement.DecreasesStock);
        Assert.Equal(-2m, movement.SignedQuantity);
        Assert.Equal(InventoryMovement.SaleType, movement.MovementType);
    }

    [Fact]
    public void ProductReturn_CreatesIncreasingProductMovement()
    {
        var movement = InventoryMovement.ProductReturn(1, 2m, "tenant-1", "return-1");

        Assert.True(movement.IncreasesStock);
        Assert.Equal(2m, movement.SignedQuantity);
        Assert.Equal(InventoryMovement.ReturnType, movement.MovementType);
    }

    [Fact]
    public void SupplyConsumption_CreatesSupplyMovement()
    {
        var movement = InventoryMovement.SupplyConsumption(7, 0.5m, "tenant-1", "recipe");

        Assert.True(movement.IsSupplyMovement);
        Assert.True(movement.DecreasesStock);
        Assert.Equal(-0.5m, movement.SignedQuantity);
        Assert.Equal(InventoryMovement.RecipeConsumptionType, movement.MovementType);
    }

    [Fact]
    public void Validate_WhenQuantityIsZero_ReturnsFailure()
    {
        var movement = new InventoryMovement
        {
            ProductId = 1,
            Quantity = 0m,
            MovementType = InventoryMovement.SaleType,
            TenantId = "tenant-1"
        };

        var result = movement.Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal("Quantity must be greater than zero.", result.Error);
    }

    [Fact]
    public void Validate_WhenMovementTypeIsInvalid_ReturnsFailure()
    {
        var movement = new InventoryMovement
        {
            ProductId = 1,
            Quantity = 1m,
            MovementType = "Unknown",
            TenantId = "tenant-1"
        };

        var result = movement.Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid inventory movement type.", result.Error);
    }

    [Fact]
    public void Validate_WhenTenantIdIsMissing_ReturnsFailure()
    {
        var movement = new InventoryMovement
        {
            ProductId = 1,
            Quantity = 1m,
            MovementType = InventoryMovement.SaleType,
            TenantId = ""
        };

        var result = movement.Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal("TenantId is required.", result.Error);
    }
}

public class InventoryMovementSignSemanticsTests
{
    [Fact]
    public void SignedQuantity_ForCanonicalPositiveSale_ReturnsNegativeQuantity()
    {
        var movement = InventoryMovement.ProductSale(1, 3m, "tenant-1", "order-1");

        Assert.True(movement.HasCanonicalPositiveStoredQuantity);
        Assert.False(movement.HasLegacyNegativeStoredQuantity);
        Assert.Equal(3m, movement.AbsoluteQuantity);
        Assert.Equal(-3m, movement.SignedQuantity);
        Assert.Equal("Decrease", movement.StockDirection);
    }

    [Fact]
    public void SignedQuantity_ForLegacyNegativeSale_RemainsNegativeAfterNormalization()
    {
        var movement = new InventoryMovement
        {
            ProductId = 1,
            Quantity = -3m,
            MovementType = InventoryMovement.SaleType,
            TenantId = "tenant-1"
        };

        Assert.True(movement.HasLegacyNegativeStoredQuantity);
        Assert.Equal(3m, movement.AbsoluteQuantity);
        Assert.Equal(-3m, movement.SignedQuantity);
        Assert.Equal("Decrease", movement.StockDirection);
    }

    [Fact]
    public void SignedQuantity_ForReturn_IsPositiveEvenWhenLegacyQuantityIsNegative()
    {
        var movement = new InventoryMovement
        {
            ProductId = 1,
            Quantity = -2m,
            MovementType = InventoryMovement.ReturnType,
            TenantId = "tenant-1"
        };

        Assert.True(movement.HasLegacyNegativeStoredQuantity);
        Assert.Equal(2m, movement.AbsoluteQuantity);
        Assert.Equal(2m, movement.SignedQuantity);
        Assert.Equal("Increase", movement.StockDirection);
    }

    [Fact]
    public void SignedQuantity_ForAdjustment_PreservesStoredSign()
    {
        var movement = new InventoryMovement
        {
            ProductId = 1,
            Quantity = -4m,
            MovementType = InventoryMovement.AdjustmentType,
            TenantId = "tenant-1"
        };

        Assert.Equal(4m, movement.AbsoluteQuantity);
        Assert.Equal(-4m, movement.SignedQuantity);
        Assert.Equal("Neutral", movement.StockDirection);
    }

    [Fact]
    public void ValidateForLedgerInterpretation_AllowsLegacyNegativeQuantity()
    {
        var movement = new InventoryMovement
        {
            ProductId = 1,
            Quantity = -1m,
            MovementType = InventoryMovement.SaleType,
            TenantId = "tenant-1"
        };

        var result = movement.ValidateForLedgerInterpretation();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateForLedgerInterpretation_WhenQuantityIsZero_ReturnsFailure()
    {
        var movement = new InventoryMovement
        {
            ProductId = 1,
            Quantity = 0m,
            MovementType = InventoryMovement.SaleType,
            TenantId = "tenant-1"
        };

        var result = movement.ValidateForLedgerInterpretation();

        Assert.False(result.IsSuccess);
        Assert.Equal("Quantity must be different from zero.", result.Error);
    }
}
