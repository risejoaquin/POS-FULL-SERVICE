using System;
using PosDomain;
using System.Collections.Generic;
namespace PosDomain.Entities;
public class CashRegisterShift
{
    public int Id { get; set; }
    public string? TenantId { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal StartingCash { get; set; }
    public decimal? ExpectedEndingCash { get; set; }
    public decimal? ActualEndingCash { get; set; }
    public decimal? Difference { get; set; }
    public string? OpenedBy { get; set; } = string.Empty;
    public string? ClosedBy { get; set; } = string.Empty;
    public bool IsClosed { get; set; } = false;
    [System.ComponentModel.DataAnnotations.Timestamp]
    public uint RowVersion { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public List<CashMovement> Movements { get; set; } = new();

    public bool IsOpen => !IsClosed && ClosedAt is null;

    public Result Close(decimal expectedEndingCash, decimal actualEndingCash, string closedBy)
    {
        if (!IsOpen)
        {
            return Result.Failure("Shift is already closed.");
        }

        if (string.IsNullOrWhiteSpace(closedBy))
        {
            return Result.Failure("Closed by is required.");
        }

        ExpectedEndingCash = expectedEndingCash;
        ActualEndingCash = actualEndingCash;
        Difference = actualEndingCash - expectedEndingCash;
        ClosedBy = closedBy;
        ClosedAt = DateTime.UtcNow;
        IsClosed = true;
        LastUpdated = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AddMovement(CashMovement movement)
    {
        if (!IsOpen)
        {
            return Result.Failure("Cannot add cash movements to a closed shift.");
        }

        var validation = movement.Validate();
        if (!validation.IsSuccess)
        {
            return validation;
        }

        Movements.Add(movement);
        LastUpdated = DateTime.UtcNow;
        return Result.Success();
    }
}
