using System;
using System.Collections.Generic;
namespace PosServer.Models;
public class CashRegisterShift
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal StartingCash { get; set; }
    public decimal? ExpectedEndingCash { get; set; }
    public decimal? ActualEndingCash { get; set; }
    public decimal? Difference { get; set; }
    public string OpenedBy { get; set; } = string.Empty;
    public string? ClosedBy { get; set; } = string.Empty;
    public bool IsClosed { get; set; } = false;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public List<CashMovement> Movements { get; set; } = new();
}
