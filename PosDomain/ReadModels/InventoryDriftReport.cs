using System.Collections.Generic;
using System.Linq;

namespace PosDomain.ReadModels
{
    /// <summary>
    /// Read-only drift report for reconstructed ledger inventory versus operational stock columns.
    /// </summary>
    public sealed class InventoryDriftReport
    {
        private readonly IReadOnlyList<InventoryDriftItem> _items;

        public InventoryDriftReport(IEnumerable<InventoryDriftItem> items)
        {
            _items = items?.ToList() ?? throw new System.ArgumentNullException(nameof(items));
        }

        public IReadOnlyList<InventoryDriftItem> Items => _items;
        public IReadOnlyList<InventoryDriftItem> DriftedItems => _items.Where(item => item.HasDrift).ToList();
        public IReadOnlyList<InventoryDriftItem> NegativeLedgerItems => _items.Where(item => item.IsNegativeLedgerBalance).ToList();
        public bool HasDrift => _items.Any(item => item.HasDrift);
        public int TotalItems => _items.Count;
        public int DriftedItemCount => _items.Count(item => item.HasDrift);
    }
}
