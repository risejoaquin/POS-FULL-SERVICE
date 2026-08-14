using PosDomain;

namespace PosDomain.Entities;

public class Supply
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = "kg";
    public decimal Cost { get; set; } = 0;
    [System.ComponentModel.DataAnnotations.ConcurrencyCheck]
    public decimal Stock { get; set; } = 0;
    public decimal MinStockThreshold { get; set; } = 0;
    
    public string TenantId { get; set; } = string.Empty;

    public bool IsLowStock => Stock <= MinStockThreshold;

    public Result CanConsume(decimal quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure("Quantity must be greater than zero.");
        }

        if (Stock < quantity)
        {
            return Result.Failure("Insufficient supply stock.");
        }

        return Result.Success();
    }

    public Result DecreaseStock(decimal quantity)
    {
        var canConsume = CanConsume(quantity);
        if (!canConsume.IsSuccess)
        {
            return canConsume;
        }

        Stock -= quantity;
        return Result.Success();
    }

    public Result IncreaseStock(decimal quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure("Quantity must be greater than zero.");
        }

        Stock += quantity;
        return Result.Success();
    }

    public Result UpdateCost(decimal cost)
    {
        if (cost < 0)
        {
            return Result.Failure("Cost cannot be negative.");
        }

        Cost = cost;
        return Result.Success();
    }

    public Result UpdateMinStockThreshold(decimal threshold)
    {
        if (threshold < 0)
        {
            return Result.Failure("Min stock threshold cannot be negative.");
        }

        MinStockThreshold = threshold;
        return Result.Success();
    }
}
