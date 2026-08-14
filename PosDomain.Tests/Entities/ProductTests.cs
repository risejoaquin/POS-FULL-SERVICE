using PosDomain.Entities;
using Xunit;

namespace PosDomain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void IsLowStock_WhenStockEqualsThreshold_ReturnsTrue()
    {
        var product = new Product { StockQuantity = 10, MinStockThreshold = 10 };

        Assert.True(product.IsLowStock);
    }

    [Fact]
    public void CanFulfill_WhenProductIsInactive_ReturnsFailure()
    {
        var product = new Product { IsActive = false, StockQuantity = 10 };

        var result = product.CanFulfill(1);

        Assert.False(result.IsSuccess);
        Assert.Equal("Product is not active.", result.Error);
    }

    [Fact]
    public void DecreaseStock_WhenStockIsAvailable_DecrementsQuantity()
    {
        var product = new Product { IsActive = true, StockQuantity = 10 };

        var result = product.DecreaseStock(3);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, product.StockQuantity);
    }

    [Fact]
    public void DecreaseStock_WhenInsufficientStock_ReturnsFailure()
    {
        var product = new Product { IsActive = true, StockQuantity = 2 };

        var result = product.DecreaseStock(3);

        Assert.False(result.IsSuccess);
        Assert.Equal("Insufficient stock.", result.Error);
        Assert.Equal(2, product.StockQuantity);
    }

    [Fact]
    public void IncreaseStock_WhenQuantityIsPositive_IncrementsQuantity()
    {
        var product = new Product { StockQuantity = 2 };

        var result = product.IncreaseStock(5);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, product.StockQuantity);
    }

    [Fact]
    public void UpdatePrice_WhenPriceIsNegative_ReturnsFailure()
    {
        var product = new Product { Price = 10m };

        var result = product.UpdatePrice(-1m);

        Assert.False(result.IsSuccess);
        Assert.Equal("Price cannot be negative.", result.Error);
        Assert.Equal(10m, product.Price);
    }
}

public class ProductAdditionalInvariantTests
{
    [Fact]
    public void ValidateForSale_WhenNameIsEmpty_ReturnsFailure()
    {
        var product = new Product { Name = "", Price = 1m, StockQuantity = 1, MinStockThreshold = 0 };

        var result = product.ValidateForSale();

        Assert.False(result.IsSuccess);
        Assert.Equal("Product name is required.", result.Error);
    }

    [Fact]
    public void UpdateMinStockThreshold_WhenNegative_ReturnsFailure()
    {
        var product = new Product { MinStockThreshold = 5 };

        var result = product.UpdateMinStockThreshold(-1);

        Assert.False(result.IsSuccess);
        Assert.Equal("Min stock threshold cannot be negative.", result.Error);
        Assert.Equal(5, product.MinStockThreshold);
    }

    [Fact]
    public void Deactivate_WhenCalled_MarksProductInactive()
    {
        var product = new Product { IsActive = true };

        product.Deactivate();

        Assert.False(product.IsActive);
    }
}
