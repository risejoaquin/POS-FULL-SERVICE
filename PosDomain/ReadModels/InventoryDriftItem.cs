using System;

namespace PosDomain.ReadModels
{
    /// <summary>
    /// Diagnostic read-only comparison between operational stock and reconstructed ledger stock.
    /// This is not a command model and must not mutate inventory.
    /// </summary>
    public sealed class InventoryDriftItem
    {
        public int EntityId { get; }
        public string EntityType { get; }
        public decimal OperationalQuantity { get; }
        public decimal LedgerQuantity { get; }
        public decimal OpeningQuantity { get; }
        public decimal MovementDelta { get; }
        public int MovementCount { get; }
        public bool HasLegacyNegativeMovement { get; }
        public bool IsNegativeLedgerBalance { get; }
        public decimal DriftQuantity => OperationalQuantity - LedgerQuantity;
        public bool HasDrift => DriftQuantity != 0m;

        public InventoryDriftItem(
            int entityId,
            string entityType,
            decimal operationalQuantity,
            InventoryLedgerBalance ledgerBalance)
        {
            if (entityId <= 0)
            {
                throw new ArgumentException("Entity id must be greater than zero.", nameof(entityId));
            }

            if (string.IsNullOrWhiteSpace(entityType))
            {
                throw new ArgumentException("Entity type is required.", nameof(entityType));
            }

            if (ledgerBalance is null)
            {
                throw new ArgumentNullException(nameof(ledgerBalance));
            }

            if (ledgerBalance.EntityId != entityId)
            {
                throw new ArgumentException("Ledger balance entity id must match drift item entity id.", nameof(ledgerBalance));
            }

            EntityId = entityId;
            EntityType = entityType.Trim();
            OperationalQuantity = operationalQuantity;
            LedgerQuantity = ledgerBalance.CurrentQuantity;
            OpeningQuantity = ledgerBalance.OpeningQuantity;
            MovementDelta = ledgerBalance.MovementDelta;
            MovementCount = ledgerBalance.MovementCount;
            HasLegacyNegativeMovement = ledgerBalance.HasLegacyNegativeMovement;
            IsNegativeLedgerBalance = ledgerBalance.IsNegative;
        }
    }
}
