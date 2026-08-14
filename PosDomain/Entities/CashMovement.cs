using System;
using PosDomain;

namespace PosDomain.Entities;

public class CashMovement
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    
    // "Entrada" o "Salida"
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public string TenantId { get; set; } = string.Empty;
    public CashRegisterShift? Shift { get; set; }


    public const string InType = "Entrada";
    public const string OutType = "Salida";

    public bool IsCashIn => string.Equals(Type, InType, StringComparison.OrdinalIgnoreCase);
    public bool IsCashOut => string.Equals(Type, OutType, StringComparison.OrdinalIgnoreCase);
    public bool HasReason => !string.IsNullOrWhiteSpace(Reason);
    public decimal SignedAmount => IsCashOut ? -Amount : Amount;

    public static CashMovement CashIn(int shiftId, decimal amount, string reason, string createdBy, string tenantId = "")
    {
        return new CashMovement
        {
            ShiftId = shiftId,
            Type = InType,
            Amount = amount,
            Reason = reason,
            CreatedBy = createdBy,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CashMovement CashOut(int shiftId, decimal amount, string reason, string createdBy, string tenantId = "")
    {
        return new CashMovement
        {
            ShiftId = shiftId,
            Type = OutType,
            Amount = amount,
            Reason = reason,
            CreatedBy = createdBy,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Result Validate()
    {
        if (ShiftId <= 0)
        {
            return Result.Failure("Shift is required.");
        }

        if (Amount <= 0)
        {
            return Result.Failure("Amount must be greater than zero.");
        }

        if (!IsCashIn && !IsCashOut)
        {
            return Result.Failure("Cash movement type must be Entrada or Salida.");
        }

        if (string.IsNullOrWhiteSpace(Reason))
        {
            return Result.Failure("Reason is required.");
        }

        return Result.Success();
    }
}
