using PosDomain;

namespace PosDomain.Entities;

public class RecipeItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public int SupplyId { get; set; }
    public Supply Supply { get; set; } = null!;
    
    public decimal Quantity { get; set; } = 1;
    
    public string TenantId { get; set; } = string.Empty;

    public Result Validate()
    {
        if (ProductId <= 0)
        {
            return Result.Failure("ProductId is required.");
        }

        if (SupplyId <= 0)
        {
            return Result.Failure("SupplyId is required.");
        }

        if (Quantity <= 0)
        {
            return Result.Failure("Recipe quantity must be greater than zero.");
        }

        return Result.Success();
    }

    public Result UpdateQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure("Recipe quantity must be greater than zero.");
        }

        Quantity = quantity;
        return Result.Success();
    }

    public decimal RequiredFor(int productQuantity)
    {
        if (productQuantity <= 0)
        {
            return 0;
        }

        return Quantity * productQuantity;
    }
}
