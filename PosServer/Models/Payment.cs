using System;
using System.Text.Json.Serialization;

namespace PosServer.Models
{
    
    public enum PaymentStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Refunded = 3
    }
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order? Order { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
        public string Method { get; set; } = string.Empty;
        public int? ShiftId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string IdempotencyKey { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public bool IsSynced { get; set; } = false;
        [System.ComponentModel.DataAnnotations.Timestamp]
    public uint RowVersion { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
