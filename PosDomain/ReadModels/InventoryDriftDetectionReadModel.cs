using System;
using System.Collections.Generic;
using System.Linq;
using PosDomain.Entities;

namespace PosDomain.ReadModels
{
    /// <summary>
    /// Detects drift between operational stock columns and ledger-reconstructed balances.
    /// This read model is diagnostic only. It does not correct, persist, or mutate inventory.
    /// </summary>
    public static class InventoryDriftDetectionReadModel
    {
        public static InventoryDriftReport DetectProductDrift(
            IEnumerable<InventoryMovement> movements,
            IReadOnlyDictionary<int, decimal> operationalQuantities,
            IReadOnlyDictionary<int, decimal>? openingQuantities = null,
            string? tenantId = null)
        {
            if (movements is null)
            {
                throw new ArgumentNullException(nameof(movements));
            }

            if (operationalQuantities is null)
            {
                throw new ArgumentNullException(nameof(operationalQuantities));
            }

            var movementList = movements.ToList();
            var entityIds = GetProductEntityIds(movementList, operationalQuantities, openingQuantities, tenantId);
            var items = entityIds
                .Select(productId => CreateProductDriftItem(movementList, productId, operationalQuantities, openingQuantities, tenantId))
                .OrderBy(item => item.EntityId)
                .ToList();

            return new InventoryDriftReport(items);
        }

        public static InventoryDriftReport DetectSupplyDrift(
            IEnumerable<InventoryMovement> movements,
            IReadOnlyDictionary<int, decimal> operationalQuantities,
            IReadOnlyDictionary<int, decimal>? openingQuantities = null,
            string? tenantId = null)
        {
            if (movements is null)
            {
                throw new ArgumentNullException(nameof(movements));
            }

            if (operationalQuantities is null)
            {
                throw new ArgumentNullException(nameof(operationalQuantities));
            }

            var movementList = movements.ToList();
            var entityIds = GetSupplyEntityIds(movementList, operationalQuantities, openingQuantities, tenantId);
            var items = entityIds
                .Select(supplyId => CreateSupplyDriftItem(movementList, supplyId, operationalQuantities, openingQuantities, tenantId))
                .OrderBy(item => item.EntityId)
                .ToList();

            return new InventoryDriftReport(items);
        }

        private static InventoryDriftItem CreateProductDriftItem(
            IEnumerable<InventoryMovement> movements,
            int productId,
            IReadOnlyDictionary<int, decimal> operationalQuantities,
            IReadOnlyDictionary<int, decimal>? openingQuantities,
            string? tenantId)
        {
            var ledgerBalance = InventoryLedgerReadModel.CalculateProductBalance(
                movements,
                productId,
                GetQuantity(openingQuantities, productId),
                tenantId);

            return new InventoryDriftItem(
                productId,
                InventoryLedgerReadModel.ProductEntityType,
                GetQuantity(operationalQuantities, productId),
                ledgerBalance);
        }

        private static InventoryDriftItem CreateSupplyDriftItem(
            IEnumerable<InventoryMovement> movements,
            int supplyId,
            IReadOnlyDictionary<int, decimal> operationalQuantities,
            IReadOnlyDictionary<int, decimal>? openingQuantities,
            string? tenantId)
        {
            var ledgerBalance = InventoryLedgerReadModel.CalculateSupplyBalance(
                movements,
                supplyId,
                GetQuantity(openingQuantities, supplyId),
                tenantId);

            return new InventoryDriftItem(
                supplyId,
                InventoryLedgerReadModel.SupplyEntityType,
                GetQuantity(operationalQuantities, supplyId),
                ledgerBalance);
        }

        private static IReadOnlyList<int> GetProductEntityIds(
            IEnumerable<InventoryMovement> movements,
            IReadOnlyDictionary<int, decimal> operationalQuantities,
            IReadOnlyDictionary<int, decimal>? openingQuantities,
            string? tenantId)
        {
            return GetBaseEntityIds(operationalQuantities, openingQuantities)
                .Concat(FilterMovements(movements, tenantId)
                    .Where(movement => movement.IsProductMovement)
                    .Select(movement => movement.ProductId))
                .Where(entityId => entityId > 0)
                .Distinct()
                .OrderBy(entityId => entityId)
                .ToList();
        }

        private static IReadOnlyList<int> GetSupplyEntityIds(
            IEnumerable<InventoryMovement> movements,
            IReadOnlyDictionary<int, decimal> operationalQuantities,
            IReadOnlyDictionary<int, decimal>? openingQuantities,
            string? tenantId)
        {
            return GetBaseEntityIds(operationalQuantities, openingQuantities)
                .Concat(FilterMovements(movements, tenantId)
                    .Where(movement => movement.SupplyId.HasValue)
                    .Select(movement => movement.SupplyId!.Value))
                .Where(entityId => entityId > 0)
                .Distinct()
                .OrderBy(entityId => entityId)
                .ToList();
        }

        private static IEnumerable<int> GetBaseEntityIds(
            IReadOnlyDictionary<int, decimal> operationalQuantities,
            IReadOnlyDictionary<int, decimal>? openingQuantities)
        {
            return operationalQuantities.Keys.Concat(openingQuantities?.Keys ?? Enumerable.Empty<int>());
        }

        private static IEnumerable<InventoryMovement> FilterMovements(IEnumerable<InventoryMovement> movements, string? tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return movements;
            }

            return movements.Where(movement => string.Equals(movement.TenantId, tenantId, StringComparison.Ordinal));
        }

        private static decimal GetQuantity(IReadOnlyDictionary<int, decimal>? quantities, int entityId)
        {
            if (quantities is null)
            {
                return 0m;
            }

            return quantities.TryGetValue(entityId, out var quantity) ? quantity : 0m;
        }
    }
}
