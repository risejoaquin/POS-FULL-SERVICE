using PosDomain;
using System.Collections.Generic;
using System.Linq;
namespace PosDomain.Entities;


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
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public string? CustomerName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Relación
    public List<OrderItem> Items { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public string IdempotencyKey { get; set; } = string.Empty;
    
    // Bandera para saber si ya se sincronizó con la BD Central
    public bool IsSynced { get; set; } = false;
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public uint RowVersion { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public bool IsReturned { get; set; } = false;
    public string ReturnReason { get; set; } = string.Empty;
    public string AuthorizedBy { get; set; } = string.Empty;
    public string CreatedById { get; set; } = string.Empty;
    public string PaymentDetails { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientSideId { get; set; } = string.Empty;

    public Dictionary<string, object> CustomAttributes { get; set; } = new();

    public Result AddItem(Product product, int quantity, decimal discount = 0, string notes = "")
    {
        if (Status != OrderStatus.Draft && Status != OrderStatus.Open)
        {
            return Result.Failure("Cannot add items to an order that is not open.");
        }

        if (quantity <= 0)
        {
            return Result.Failure("Quantity must be greater than zero.");
        }

        var item = new OrderItem
        {
            ProductId = product.Id,
            ProductBarcode = product.Barcode,
            Quantity = quantity,
            UnitPrice = product.Price,
            Discount = discount,
            Notes = notes,
            TenantId = this.TenantId
        };

        Items.Add(item);
        CalculateTotal();

        return Result.Success();
    }

    public void CalculateTotal()
    {
        SubTotal = 0;
        foreach (var item in Items)
        {
            SubTotal += item.SubTotal;
        }
        
        TaxAmount = SubTotal * 0.16m; // Assuming 16% tax for example, or could be 0
        TotalAmount = SubTotal + TaxAmount;
    }

    public Result Complete(string authorizedBy)
    {
        if (Items.Count == 0)
        {
            return Result.Failure("Order must contain at least one item.");
        }

        Status = OrderStatus.Closed;
        AuthorizedBy = authorizedBy;
        LastUpdated = DateTime.UtcNow;

        return Result.Success();
    }


    public bool HasItems => Items.Count > 0;
    public bool IsClosed => Status == OrderStatus.Closed;
    public bool IsRefunded => Status == OrderStatus.Refunded || IsReturned;
    public decimal TotalPaid => Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
    public decimal CashPaid => Payments.Where(p => p.Status == PaymentStatus.Completed && p.IsCash).Sum(p => p.Amount);
    public decimal CardPaid => Payments.Where(p => p.Status == PaymentStatus.Completed && p.IsCard).Sum(p => p.Amount);
    public decimal BalanceDue => TotalAmount - TotalPaid;
    public bool IsFullyPaid => TotalAmount > 0 && TotalPaid >= TotalAmount;

    public Result AddPayment(Payment payment)
    {
        if (payment.Amount <= 0)
        {
            return Result.Failure("Payment amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(payment.Method))
        {
            return Result.Failure("Payment method is required.");
        }

        if (payment.Status == PaymentStatus.Failed)
        {
            return Result.Failure("Failed payments cannot be added to an order.");
        }

        payment.TenantId = TenantId;
        Payments.Add(payment);
        LastUpdated = DateTime.UtcNow;

        return Result.Success();
    }

    public Result MarkRefunded(string reason, string authorizedBy)
    {
        if (IsRefunded)
        {
            return Result.Failure("Order is already refunded.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure("Return reason is required.");
        }

        if (string.IsNullOrWhiteSpace(authorizedBy))
        {
            return Result.Failure("Authorized by is required.");
        }

        Status = OrderStatus.Refunded;
        IsReturned = true;
        ReturnReason = reason;
        AuthorizedBy = authorizedBy;
        LastUpdated = DateTime.UtcNow;

        return Result.Success();
    }
}
