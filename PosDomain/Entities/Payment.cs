using System;
using PosDomain;

namespace PosDomain.Entities
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
        public const string CashMethod = "Efectivo";
        public const string CardMethod = "Tarjeta";

        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
        public string Method { get; set; } = string.Empty;
        public int? ShiftId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string IdempotencyKey { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public bool IsSynced { get; set; } = false;
        [System.ComponentModel.DataAnnotations.Timestamp]
    public uint RowVersion { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;

        public bool IsCash => string.Equals(Method, CashMethod, StringComparison.OrdinalIgnoreCase);
        public bool IsCard => string.Equals(Method, CardMethod, StringComparison.OrdinalIgnoreCase);
        public bool IsCompleted => Status == PaymentStatus.Completed;
        public bool IsRefunded => Status == PaymentStatus.Refunded;
        public bool IsPending => Status == PaymentStatus.Pending;

        public decimal SignedAmount => IsRefunded ? -Amount : Amount;

        public Result MarkCompleted()
        {
            if (Amount <= 0)
            {
                return Result.Failure("Payment amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(Method))
            {
                return Result.Failure("Payment method is required.");
            }

            Status = PaymentStatus.Completed;
            LastUpdated = DateTime.UtcNow;
            return Result.Success();
        }

        public Result MarkRefunded()
        {
            if (IsRefunded)
            {
                return Result.Failure("Payment is already refunded.");
            }

            Status = PaymentStatus.Refunded;
            LastUpdated = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Validate()
        {
            if (OrderId <= 0)
            {
                return Result.Failure("Order is required.");
            }

            if (Amount <= 0)
            {
                return Result.Failure("Payment amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(Method))
            {
                return Result.Failure("Payment method is required.");
            }

            return Result.Success();
        }

    }
}
