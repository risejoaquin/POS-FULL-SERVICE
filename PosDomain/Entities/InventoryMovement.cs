using System;
using PosDomain;

namespace PosDomain.Entities
{
    public class InventoryMovement
    {
        public const string SaleType = "Sale";
        public const string RestockType = "Restock";
        public const string ReturnType = "Return";
        public const string AdjustmentType = "Adjustment";
        public const string RecipeConsumptionType = "RecipeConsumption";

        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? SupplyId { get; set; }
        public decimal Quantity { get; set; }
        public string MovementType { get; set; } = SaleType; // Sale, Restock, Return
        public DateTime MovementDate { get; set; } = DateTime.Now;
        public string Reference { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public bool IsSynced { get; set; } = false;
        [System.ComponentModel.DataAnnotations.Timestamp]
        public uint RowVersion { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public bool IsProductMovement => ProductId > 0 && SupplyId is null;
        public bool IsSupplyMovement => SupplyId.HasValue;
        public bool DecreasesStock => MovementType is SaleType or RecipeConsumptionType;
        public bool IncreasesStock => MovementType is RestockType or ReturnType;
        public bool HasLegacyNegativeStoredQuantity => Quantity < 0;
        public bool HasCanonicalPositiveStoredQuantity => Quantity > 0;
        public decimal AbsoluteQuantity => Math.Abs(Quantity);
        public decimal SignedQuantity => ToSignedQuantity();
        public string StockDirection => DecreasesStock ? "Decrease" : IncreasesStock ? "Increase" : "Neutral";

        public static InventoryMovement ProductSale(int productId, decimal quantity, string tenantId, string reference = "")
        {
            return Create(productId, null, quantity, SaleType, tenantId, reference);
        }

        public static InventoryMovement ProductReturn(int productId, decimal quantity, string tenantId, string reference = "")
        {
            return Create(productId, null, quantity, ReturnType, tenantId, reference);
        }

        public static InventoryMovement ProductRestock(int productId, decimal quantity, string tenantId, string reference = "")
        {
            return Create(productId, null, quantity, RestockType, tenantId, reference);
        }

        public static InventoryMovement SupplyConsumption(int supplyId, decimal quantity, string tenantId, string reference = "")
        {
            return Create(0, supplyId, quantity, RecipeConsumptionType, tenantId, reference);
        }

        public Result Validate()
        {
            if (ProductId <= 0 && !SupplyId.HasValue)
            {
                return Result.Failure("ProductId or SupplyId is required.");
            }

            if (Quantity <= 0)
            {
                return Result.Failure("Quantity must be greater than zero.");
            }

            if (!IsKnownMovementType(MovementType))
            {
                return Result.Failure("Invalid inventory movement type.");
            }

            if (string.IsNullOrWhiteSpace(TenantId))
            {
                return Result.Failure("TenantId is required.");
            }

            return Result.Success();
        }

        public Result ValidateForLedgerInterpretation()
        {
            if (ProductId <= 0 && !SupplyId.HasValue)
            {
                return Result.Failure("ProductId or SupplyId is required.");
            }

            if (Quantity == 0)
            {
                return Result.Failure("Quantity must be different from zero.");
            }

            if (!IsKnownMovementType(MovementType))
            {
                return Result.Failure("Invalid inventory movement type.");
            }

            if (string.IsNullOrWhiteSpace(TenantId))
            {
                return Result.Failure("TenantId is required.");
            }

            return Result.Success();
        }

        public decimal ToSignedQuantity()
        {
            var absoluteQuantity = AbsoluteQuantity;

            if (DecreasesStock)
            {
                return -absoluteQuantity;
            }

            if (IncreasesStock)
            {
                return absoluteQuantity;
            }

            return Quantity;
        }

        private static InventoryMovement Create(int productId, int? supplyId, decimal quantity, string movementType, string tenantId, string reference)
        {
            return new InventoryMovement
            {
                ProductId = productId,
                SupplyId = supplyId,
                Quantity = quantity,
                MovementType = movementType,
                TenantId = tenantId,
                Reference = reference,
                MovementDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
        }

        private static bool IsKnownMovementType(string movementType)
        {
            return movementType is SaleType or RestockType or ReturnType or AdjustmentType or RecipeConsumptionType;
        }
    }
}
