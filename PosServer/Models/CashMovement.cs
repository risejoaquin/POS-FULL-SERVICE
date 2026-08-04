using System;
namespace PosServer.Models;
public class CashMovement
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public CashRegisterShift? Shift { get; set; }
}
