using System;
using System.Collections.Generic;
using System.Linq;
using PosDomain.Entities;

namespace PosDomain.ReadModels
{
    /// <summary>
    /// Reconstructs inventory balances from InventoryMovement entries using SignedQuantity.
    /// This read model is intentionally side-effect free and does not mutate Product, Supply, or InventoryMovement.
    /// </summary>
    public static class InventoryLedgerReadModel
    {
        public const string ProductEntityType = "Product";
        public const string SupplyEntityType = "Supply";

        public static InventoryLedgerBalance CalculateProductBalance(
            IEnumerable<InventoryMovement> movements,
            int productId,
            decimal openingQuantity = 0m,
            string? tenantId = null)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Product id must be greater than zero.", nameof(productId));
            }

            var relevantMovements = FilterMovements(movements, tenantId)
                .Where(movement => movement.ProductId == productId && movement.SupplyId is null)
                .ToList();

            return BuildBalance(productId, ProductEntityType, openingQuantity, relevantMovements);
        }

        public static InventoryLedgerBalance CalculateSupplyBalance(
            IEnumerable<InventoryMovement> movements,
            int supplyId,
            decimal openingQuantity = 0m,
            string? tenantId = null)
        {
            if (supplyId <= 0)
            {
                throw new ArgumentException("Supply id must be greater than zero.", nameof(supplyId));
            }

            var relevantMovements = FilterMovements(movements, tenantId)
                .Where(movement => movement.SupplyId == supplyId)
                .ToList();

            return BuildBalance(supplyId, SupplyEntityType, openingQuantity, relevantMovements);
        }

        public static IReadOnlyDictionary<int, InventoryLedgerBalance> BuildProductBalances(
            IEnumerable<InventoryMovement> movements,
            IReadOnlyDictionary<int, decimal>? openingQuantities = null,
            string? tenantId = null)
        {
            var relevantMovements = FilterMovements(movements, tenantId)
                .Where(movement => movement.IsProductMovement)
                .GroupBy(movement => movement.ProductId);

            return relevantMovements.ToDictionary(
                group => group.Key,
                group => BuildBalance(group.Key, ProductEntityType, GetOpeningQuantity(openingQuantities, group.Key), group));
        }

        public static IReadOnlyDictionary<int, InventoryLedgerBalance> BuildSupplyBalances(
            IEnumerable<InventoryMovement> movements,
            IReadOnlyDictionary<int, decimal>? openingQuantities = null,
            string? tenantId = null)
        {
            var relevantMovements = FilterMovements(movements, tenantId)
                .Where(movement => movement.SupplyId.HasValue)
                .GroupBy(movement => movement.SupplyId!.Value);

            return relevantMovements.ToDictionary(
                group => group.Key,
                group => BuildBalance(group.Key, SupplyEntityType, GetOpeningQuantity(openingQuantities, group.Key), group));
        }

        private static InventoryLedgerBalance BuildBalance(
            int entityId,
            string entityType,
            decimal openingQuantity,
            IEnumerable<InventoryMovement> movements)
        {
            var movementList = movements.ToList();

            foreach (var movement in movementList)
            {
                var validation = movement.ValidateForLedgerInterpretation();
                if (!validation.IsSuccess)
                {
                    throw new InvalidOperationException(validation.Error);
                }
            }

            return new InventoryLedgerBalance(
                entityId,
                entityType,
                openingQuantity,
                movementList.Sum(movement => movement.SignedQuantity),
                movementList.Count,
                movementList.Any(movement => movement.HasLegacyNegativeStoredQuantity));
        }

        private static IEnumerable<InventoryMovement> FilterMovements(IEnumerable<InventoryMovement> movements, string? tenantId)
        {
            if (movements is null)
            {
                throw new ArgumentNullException(nameof(movements));
            }

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return movements;
            }

            return movements.Where(movement => string.Equals(movement.TenantId, tenantId, StringComparison.Ordinal));
        }

        private static decimal GetOpeningQuantity(IReadOnlyDictionary<int, decimal>? openingQuantities, int entityId)
        {
            if (openingQuantities is null)
            {
                return 0m;
            }

            return openingQuantities.TryGetValue(entityId, out var openingQuantity) ? openingQuantity : 0m;
        }
    }
}
