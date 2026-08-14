using System;
using System.Collections.Generic;

namespace PosApplication.DTOs.Local
{
    public class CheckoutRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;
        public string IdempotencyKey { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TotalTendered { get; set; }
        public decimal ChangeDue { get; set; }
        public List<CheckoutLineRequest> Lines { get; set; } = new();
        public List<CheckoutPaymentRequest> Payments { get; set; } = new();
    }
}
