using PosDomain.Entities;
using Xunit;

namespace PosDomain.Tests.Entities;

public class RecipeItemTests
{
    [Fact]
    public void Validate_WhenQuantityIsPositive_ReturnsSuccess()
    {
        var item = new RecipeItem { ProductId = 1, SupplyId = 2, Quantity = 0.25m };

        var result = item.Validate();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_WhenQuantityIsZero_ReturnsFailure()
    {
        var item = new RecipeItem { ProductId = 1, SupplyId = 2, Quantity = 0m };

        var result = item.Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal("Recipe quantity must be greater than zero.", result.Error);
    }

    [Fact]
    public void UpdateQuantity_WhenQuantityIsPositive_UpdatesQuantity()
    {
        var item = new RecipeItem { Quantity = 1m };

        var result = item.UpdateQuantity(2.5m);

        Assert.True(result.IsSuccess);
        Assert.Equal(2.5m, item.Quantity);
    }

    [Fact]
    public void RequiredFor_WhenProductQuantityIsPositive_ReturnsRequiredSupplyQuantity()
    {
        var item = new RecipeItem { Quantity = 0.25m };

        var required = item.RequiredFor(4);

        Assert.Equal(1m, required);
    }

    [Fact]
    public void RequiredFor_WhenProductQuantityIsZero_ReturnsZero()
    {
        var item = new RecipeItem { Quantity = 0.25m };

        var required = item.RequiredFor(0);

        Assert.Equal(0m, required);
    }
}
