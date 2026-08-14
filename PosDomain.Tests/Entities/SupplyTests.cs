using PosDomain.Entities;
using Xunit;

namespace PosDomain.Tests.Entities;

public class SupplyTests
{
    [Fact]
    public void IsLowStock_WhenStockEqualsThreshold_ReturnsTrue()
    {
        var supply = new Supply { Stock = 5m, MinStockThreshold = 5m };

        Assert.True(supply.IsLowStock);
    }

    [Fact]
    public void DecreaseStock_WhenAvailable_DecrementsStock()
    {
        var supply = new Supply { Stock = 10m };

        var result = supply.DecreaseStock(2.5m);

        Assert.True(result.IsSuccess);
        Assert.Equal(7.5m, supply.Stock);
    }

    [Fact]
    public void DecreaseStock_WhenInsufficient_ReturnsFailure()
    {
        var supply = new Supply { Stock = 1m };

        var result = supply.DecreaseStock(2m);

        Assert.False(result.IsSuccess);
        Assert.Equal("Insufficient supply stock.", result.Error);
        Assert.Equal(1m, supply.Stock);
    }

    [Fact]
    public void IncreaseStock_WhenQuantityIsPositive_IncrementsStock()
    {
        var supply = new Supply { Stock = 1m };

        var result = supply.IncreaseStock(4m);

        Assert.True(result.IsSuccess);
        Assert.Equal(5m, supply.Stock);
    }

    [Fact]
    public void UpdateCost_WhenNegative_ReturnsFailure()
    {
        var supply = new Supply { Cost = 10m };

        var result = supply.UpdateCost(-1m);

        Assert.False(result.IsSuccess);
        Assert.Equal("Cost cannot be negative.", result.Error);
        Assert.Equal(10m, supply.Cost);
    }

    [Fact]
    public void UpdateMinStockThreshold_WhenNegative_ReturnsFailure()
    {
        var supply = new Supply { MinStockThreshold = 3m };

        var result = supply.UpdateMinStockThreshold(-1m);

        Assert.False(result.IsSuccess);
        Assert.Equal("Min stock threshold cannot be negative.", result.Error);
        Assert.Equal(3m, supply.MinStockThreshold);
    }
}
