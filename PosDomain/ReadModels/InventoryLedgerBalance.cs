using System;

namespace PosDomain.ReadModels
{
    /// <summary>
    /// Read-only inventory balance reconstructed from ledger movements.
    /// This is not a persistence model and does not replace the current stock columns yet.
    /// </summary>
    public sealed class InventoryLedgerBalance
    {
        public int EntityId { get; }
        public string EntityType { get; }
        public decimal OpeningQuantity { get; }
        public decimal MovementDelta { get; }
        public decimal CurrentQuantity => OpeningQuantity + MovementDelta;
        public int MovementCount { get; }
        public bool HasLegacyNegativeMovement { get; }
        public bool IsNegative => CurrentQuantity < 0;

        public InventoryLedgerBalance(
            int entityId,
            string entityType,
            decimal openingQuantity,
            decimal movementDelta,
            int movementCount,
            bool hasLegacyNegativeMovement)
        {
            if (entityId <= 0)
            {
                throw new ArgumentException("Entity id must be greater than zero.", nameof(entityId));
            }

            if (string.IsNullOrWhiteSpace(entityType))
            {
                throw new ArgumentException("Entity type is required.", nameof(entityType));
            }

            EntityId = entityId;
            EntityType = entityType.Trim();
            OpeningQuantity = openingQuantity;
            MovementDelta = movementDelta;
            MovementCount = movementCount;
            HasLegacyNegativeMovement = hasLegacyNegativeMovement;
        }
    }
}
