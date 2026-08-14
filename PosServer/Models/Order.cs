using System;
using System.Collections.Generic;
namespace PosServer.Models;

    public enum OrderStatus
    {
        Draft = 0,
        Open = 1,
        Paid = 2,
        Closed = 3,
        Cancelled = 4,
        Refunded = 5
    }

public class Order
{
    public int Id { get; set; }
    public int? ShiftId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Closed;
    public DateTime OrderDate { get; set; }
    public string? CustomerName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public bool IsSynced { get; set; } = false;
    [System.ComponentModel.DataAnnotations.Timestamp]
    public uint RowVersion { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsReturned { get; set; } = false;
    public string? ReturnReason { get; set; } = string.Empty;
    public string? AuthorizedBy { get; set; } = string.Empty;
    public string? PaymentDetails { get; set; } = string.Empty;

    public string? TenantId { get; set; } = string.Empty;
    public string? ClientSideId { get; set; } = string.Empty;
    public Dictionary<string, object> CustomAttributes { get; set; } = new();
    public List<OrderItem> Items { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public string IdempotencyKey { get; set; } = string.Empty;
}
