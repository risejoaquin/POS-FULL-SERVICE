using PosDomain;
using System.Collections.Generic;
namespace PosDomain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    [System.ComponentModel.DataAnnotations.ConcurrencyCheck]
    public int StockQuantity { get; set; }
    public int MinStockThreshold { get; set; } = 10;
    public string Category { get; set; } = "General";
    public string ImagePath { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public uint RowVersion { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public string TenantId { get; set; } = string.Empty;
    public Dictionary<string, object> CustomAttributes { get; set; } = new();
    
    public ICollection<ProductModifierLink> ProductModifiers { get; set; } = new List<ProductModifierLink>();
    public ICollection<RecipeItem> RecipeItems { get; set; } = new List<RecipeItem>();

    public bool IsLowStock => StockQuantity <= MinStockThreshold;

    public Result CanFulfill(int quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure("Quantity must be greater than zero.");
        }

        if (!IsActive)
        {
            return Result.Failure("Product is not active.");
        }

        if (StockQuantity < quantity)
        {
            return Result.Failure("Insufficient stock.");
        }

        return Result.Success();
    }

    public Result DecreaseStock(int quantity)
    {
        var canFulfill = CanFulfill(quantity);
        if (!canFulfill.IsSuccess)
        {
            return canFulfill;
        }

        StockQuantity -= quantity;
        LastUpdated = DateTime.UtcNow;
        return Result.Success();
    }

    public Result IncreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure("Quantity must be greater than zero.");
        }

        StockQuantity += quantity;
        LastUpdated = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdatePrice(decimal price)
    {
        if (price < 0)
        {
            return Result.Failure("Price cannot be negative.");
        }

        Price = price;
        LastUpdated = DateTime.UtcNow;
        return Result.Success();
    }

    public Result ValidateForSale()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return Result.Failure("Product name is required.");
        }

        if (Price < 0)
        {
            return Result.Failure("Price cannot be negative.");
        }

        if (StockQuantity < 0)
        {
            return Result.Failure("Stock quantity cannot be negative.");
        }

        if (MinStockThreshold < 0)
        {
            return Result.Failure("Min stock threshold cannot be negative.");
        }

        return Result.Success();
    }

    public Result UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure("Product name is required.");
        }

        Name = name.Trim();
        LastUpdated = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateMinStockThreshold(int threshold)
    {
        if (threshold < 0)
        {
            return Result.Failure("Min stock threshold cannot be negative.");
        }

        MinStockThreshold = threshold;
        LastUpdated = DateTime.UtcNow;
        return Result.Success();
    }

    public void Activate()
    {
        IsActive = true;
        LastUpdated = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastUpdated = DateTime.UtcNow;
    }

}
