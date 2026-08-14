using System;

namespace PosServer.Models
{
    public class InventoryMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? SupplyId { get; set; }
        public decimal Quantity { get; set; }
        public string MovementType { get; set; } = "Sale"; // Sale, Restock, Return
        public DateTime MovementDate { get; set; } = DateTime.UtcNow;
        public string Reference { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public bool IsSynced { get; set; } = false;
        [System.ComponentModel.DataAnnotations.Timestamp]
    public uint RowVersion { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
